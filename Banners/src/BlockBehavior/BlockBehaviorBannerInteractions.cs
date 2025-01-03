using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Flags;

public class BlockBehaviorBannerInteractions : BlockBehavior
{
    public BlockBanner blockBanner => block as BlockBanner;

    public BlockBehaviorBannerInteractions(Block block) : base(block) { }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        if (blockSel == null || world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityBanner blockEntity || blockSel.IsProtected(world, byPlayer, EnumBlockAccessFlags.BuildOrBreak))
        {
            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }

        ItemSlot leftSlot = byPlayer.Entity.LeftHandItemSlot;
        ItemSlot rightSlot = byPlayer.Entity.RightHandItemSlot;

        if (AddLayer(byPlayer, leftSlot, rightSlot, blockEntity.BannerProps, blockEntity)
            || RemoveLayer(byPlayer, rightSlot, blockEntity.BannerProps, blockEntity)
            || AddCutout(byPlayer, leftSlot, rightSlot, blockEntity.BannerProps, blockEntity)
            || RemoveCutout(byPlayer, leftSlot, rightSlot, blockEntity.BannerProps, blockEntity)
            || CopyLayers(byPlayer, rightSlot, blockEntity.BannerProps, blockEntity)
            || Rename(byPlayer, rightSlot, blockEntity.BannerProps, blockEntity))
        {
            blockEntity.MarkDirty(true);
            byPlayer.Entity.RightHandItemSlot.MarkDirty();
            byPlayer.Entity.LeftHandItemSlot.MarkDirty();
            handling = EnumHandling.PreventDefault;
            return true;
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
    }

    public bool AddLayer(IPlayer byPlayer, ItemSlot leftSlot, ItemSlot rightSlot, BannerProperties bannerProps, BlockEntityBanner blockEntityBanner, bool isPreview = false)
    {
        if (leftSlot?.Itemstack?.Collectible is not ItemBannerPattern itemPattern
            || rightSlot?.Itemstack?.Collectible is not BlockLiquidContainerTopOpened blockContainer
            || !blockEntityBanner.IsEditModeEnabled(printError: !isPreview))
        {
            return false;
        }

        PatternProperties patternProperties = PatternProperties.FromStack(leftSlot.Itemstack);
        if (string.IsNullOrEmpty(patternProperties.Type))
        {
            return false;
        }

        if (!blockBanner.MatchesPatternGroups(itemPattern, patternProperties))
        {
            if (!isPreview) byPlayer.IngameError(blockBanner, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
            return false;
        }
        if (!BannerLiquid.TryGet(rightSlot.Itemstack, blockContainer, out BannerLiquid liquidProps) || !liquidProps.IsDye)
        {
            return false;
        }

        if (!liquidProps.CanTakeLiquid(rightSlot.Itemstack, blockContainer) && !byPlayer.IsCreative())
        {
            if (!isPreview) byPlayer.IngameError(blockBanner, IngameError.BannerNotEnoughDye, IngameError.BannerNotEnoughDye.Localize(liquidProps.LitresPerUse));
            return false;
        }

        if (!bannerProps.Patterns.TryAdd(new BannerLayer().WithPattern(patternProperties.Type).WithColor(liquidProps.Color), byPlayer.Entity.World, byPlayer))
        {
            if (!isPreview) byPlayer.IngameError(blockBanner, IngameError.LayersLimitReached, IngameError.LayersLimitReached.Localize(Patterns.GetLayersLimit(byPlayer.Entity.World)));
            return false;
        }
        return isPreview ? true : liquidProps.TryTakeLiquid(byPlayer, rightSlot, blockContainer);
    }

    public bool RemoveLayer(IPlayer byPlayer, ItemSlot rightSlot, BannerProperties bannerProps, BlockEntityBanner blockEntityBanner, bool isPreview = false)
    {
        if (rightSlot?.Itemstack?.Collectible is not BlockLiquidContainerTopOpened blockContainer
            || !blockEntityBanner.IsEditModeEnabled(printError: !isPreview))
        {
            return false;
        }
        if (!BannerLiquid.TryGet(rightSlot.Itemstack, blockContainer, out BannerLiquid liquidProps) || !liquidProps.IsBleach)
        {
            return false;
        }
        if (!liquidProps.CanTakeLiquid(rightSlot.Itemstack, blockContainer) && !byPlayer.IsCreative())
        {
            if (!isPreview) byPlayer.IngameError(blockBanner, IngameError.BannerNotEnoughBleach, IngameError.BannerNotEnoughBleach.Localize(liquidProps.LitresPerUse));
            return false;
        }

        if (!bannerProps.Patterns.TryRemoveLast())
        {
            return false;
        }
        return isPreview ? true : liquidProps.TryTakeLiquid(byPlayer, rightSlot, blockContainer);
    }

    public bool AddCutout(IPlayer byPlayer, ItemSlot leftSlot, ItemSlot rightSlot, BannerProperties bannerProps, BlockEntityBanner blockEntityBanner, bool isPreview = false)
    {
        CollectibleBehaviorCutoutTool cutoutToolBehavior = rightSlot?.Itemstack?.Collectible?.GetBehavior<CollectibleBehaviorCutoutTool>();
        if (leftSlot?.Itemstack?.Collectible is not ItemBannerPattern itemPattern || cutoutToolBehavior == null)
        {
            return false;
        }

        if (!blockEntityBanner.IsEditModeEnabled(printError: !isPreview)) return false;

        PatternProperties patternProperties = PatternProperties.FromStack(leftSlot.Itemstack);
        if (string.IsNullOrEmpty(patternProperties.Type))
        {
            return false;
        }

        if (!blockEntityBanner.BannerBlock.MatchesPatternGroups(itemPattern, patternProperties))
        {
            if (!isPreview) byPlayer.IngameError(blockBanner, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
            return false;
        }

        bool applied = cutoutToolBehavior.HasEnoughDurability(rightSlot) && bannerProps.Cutouts.TryAdd(new BannerLayer().WithPattern(patternProperties.Type));

        if (applied && !isPreview)
        {
            cutoutToolBehavior.ConsumeDurability(byPlayer.Entity.World, byPlayer.Entity, rightSlot);
        }
        if (applied && !isPreview) cutoutToolBehavior.PlayCutSound(byPlayer);
        return applied;
    }

    public static bool RemoveCutout(IPlayer byPlayer, ItemSlot leftSlot, ItemSlot rightSlot, BannerProperties bannerProps, BlockEntityBanner blockEntityBanner, bool isPreview = false)
    {
        CollectibleBehaviorCutoutTool cutoutToolBehavior = rightSlot?.Itemstack?.Collectible?.GetBehavior<CollectibleBehaviorCutoutTool>();
        if (!leftSlot.Empty || cutoutToolBehavior == null)
        {
            return false;
        }

        if (!blockEntityBanner.IsEditModeEnabled(printError: !isPreview)) return false;

        bool applied = bannerProps.Cutouts.TryRemoveLast();
        if (applied && !isPreview) cutoutToolBehavior.PlayRepairSound(byPlayer);
        return applied;
    }

    public static bool CopyLayers(IPlayer byPlayer, ItemSlot rightSlot, BannerProperties bannerProps, BlockEntityBanner blockEntityBanner, bool isPreview = false)
    {
        if (rightSlot?.Itemstack?.Collectible is not BlockBanner anotherBlockBanner)
        {
            return false;
        }

        if (!blockEntityBanner.BannerBlock.MatchesPatternGroups(anotherBlockBanner))
        {
            if (!isPreview) byPlayer.IngameError(blockEntityBanner.BannerBlock, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
            return false;
        }

        if (bannerProps.CopyTo(rightSlot.Itemstack, copyLayers: true, copyCutouts: true))
        {
            return true;
        }

        if (!blockEntityBanner.IsEditModeEnabled(printError: !isPreview)) return false;

        if (bannerProps.CopyFrom(rightSlot.Itemstack, copyLayers: true, copyCutouts: true))
        {
            return true;
        }
        if (!isPreview) byPlayer.IngameError(blockEntityBanner, IngameError.BannerCopyLayers, IngameError.BannerCopyLayers.Localize());
        return false;
    }

    public bool Rename(IPlayer byPlayer, ItemSlot rightSlot, BannerProperties bannerProps, BlockEntityBanner blockEntityBanner)
    {
        CollectibleBehaviorRenameTool renameToolBehavior = rightSlot?.Itemstack?.Collectible?.GetBehavior<CollectibleBehaviorRenameTool>();
        if (renameToolBehavior == null || !blockEntityBanner.IsEditModeEnabled())
        {
            return false;
        }

        string newName = rightSlot.Itemstack.Attributes.GetString(renameToolBehavior.FromAttribute);
        if (string.IsNullOrEmpty(newName))
        {
            byPlayer.IngameError(blockBanner, IngameError.BannerRename, IngameError.BannerRename.Localize());
            return false;
        }

        bannerProps.SetName(newName);
        return true;
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection blockSel, IPlayer forPlayer, ref EnumHandling handling)
    {
        if (world.Api is not ICoreClientAPI capi || world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityBanner blockEntity)
        {
            return Array.Empty<WorldInteraction>();
        }

        handling = EnumHandling.Handled;
        return BannerInteractions(blockEntity, capi, blockSel);
    }

    public WorldInteraction[] BannerInteractions(BlockEntityBanner blockEntityBanner, ICoreClientAPI capi, BlockSelection blockSel)
    {
        List<WorldInteraction> interactions = new List<WorldInteraction>();

        ItemStack[] bannerStacks = Array.Empty<ItemStack>();
        foreach (ItemStack stack in ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyBannerStacks))
        {
            BannerProperties stackProps = BannerProperties.FromStack(stack);
            if (blockEntityBanner.BannerProps.Patterns.SameBaseColors(stackProps) && stackProps.Patterns.Count == 1)
            {
                bannerStacks = bannerStacks.Append(stack);
            }
        }

        interactions.Add(new WorldInteraction()
        {
            ActionLangCode = blockEntityBanner.IsEditModeEnabled(printError: false) ? langCodeCopyLayers : langCodeCopyLayersFromPlaced,
            MouseButton = EnumMouseButton.Right,
            Itemstacks = bannerStacks
        });

        if (blockEntityBanner.IsEditModeEnabled(printError: false))
        {
            interactions.Add(new WorldInteraction()
            {
                ActionLangCode = langCodeAddLayer,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyDyeStacks)
            });
            interactions.Add(new WorldInteraction()
            {
                ActionLangCode = langCodeRemoveLayer,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyBleachStacks)
            });
            interactions.Add(new WorldInteraction()
            {
                ActionLangCode = langCodeAddCutout,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyShearsStacks)
            });
            interactions.Add(new WorldInteraction()
            {
                ActionLangCode = langCodeRemoveCutout,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyShearsStacks)
            });
            interactions.Add(new WorldInteraction()
            {
                ActionLangCode = langCodeRename,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyBookStacks)
            });
            IRotatableBanner rotatableBanner = blockSel.Block.GetInterface<IRotatableBanner>(capi.World, blockSel.Position);
            if (rotatableBanner != null)
            {
                interactions.AddRange(ObjectCacheUtil.TryGet<WorldInteraction[]>(capi, cacheKeyRotatableBannerInteractions));
            }
            if (capi.World.BlockAccessor.TryGetBEBehavior(blockSel, out BEBehaviorWrenchOrientableBanner banner))
            {
                interactions.Add(ObjectCacheUtil.TryGet<WorldInteraction>(capi, cacheKeyWrenchableBannerInteractions));
            }
        }

        return interactions.ToArray();
    }

    public ItemStack GetPreview(IPlayer player, BlockSelection blockSel)
    {
        IWorldAccessor world = player.Entity.World;

        if (blockSel == null
            || world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityBanner blockEntity
            || blockSel.IsProtected(world, player, EnumBlockAccessFlags.BuildOrBreak))
        {
            return null;
        }
        ItemStack placedStack = blockEntity.BannerBlock?.OnPickBlock(world, blockSel.Position);
        BannerProperties placedProps = BannerProperties.FromStack(placedStack);

        ItemSlot leftSlot = new DummySlot(player.Entity?.LeftHandItemSlot?.Itemstack?.Clone());
        ItemSlot rightSlot = new DummySlot(player.Entity?.RightHandItemSlot?.Itemstack?.Clone());

        if (AddLayer(player, leftSlot, rightSlot, placedProps, blockEntity, isPreview: true)
            || RemoveLayer(player, rightSlot, placedProps, blockEntity, isPreview: true)
            || AddCutout(player, leftSlot, rightSlot, placedProps, blockEntity, isPreview: true)
            || RemoveCutout(player, leftSlot, rightSlot, placedProps, blockEntity, isPreview: true)
            || CopyLayers(player, rightSlot, placedProps, blockEntity, isPreview: true))
        {
            placedStack.Attributes.RemoveAttribute(attributeBanner);
            placedProps.ToStack(placedStack);
            return placedStack;
        }
        return null;
    }
}