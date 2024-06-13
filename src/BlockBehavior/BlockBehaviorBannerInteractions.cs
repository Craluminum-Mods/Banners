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
    public BlockBehaviorBannerInteractions(Block block) : base(block) { }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        if (blockSel == null || world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityBanner blockEntity || blockSel.IsProtected(world, byPlayer, EnumBlockAccessFlags.BuildOrBreak))
        {
            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }

        if (AddLayer(world, byPlayer, blockEntity) || RemoveLayer(byPlayer, blockEntity) || AddCutout(byPlayer, blockEntity) || RemoveCutout(byPlayer, blockEntity) || CopyLayers(byPlayer, blockEntity) || Rename(byPlayer, blockEntity))
        {
            blockEntity.MarkDirty(true);
            byPlayer.Entity.RightHandItemSlot.MarkDirty();
            byPlayer.Entity.LeftHandItemSlot.MarkDirty();
            handling = EnumHandling.PreventDefault;
            return true;
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
    }

    public bool AddLayer(IWorldAccessor world, IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;
        ItemSlot offHandSlot = byPlayer.Entity.LeftHandItemSlot;

        if (offHandSlot?.Itemstack?.Collectible is not ItemBannerPattern itemPattern || activeSlot?.Itemstack?.Collectible is not BlockLiquidContainerTopOpened blockContainer)
        {
            return false;
        }

        if (!blockEntity.IsEditModeEnabled(byPlayer)) return false;

        if (!blockEntity.BannerBlock.MatchesPatternGroups(itemPattern))
        {
            byPlayer.IngameError(this, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
            return false;
        }
        if (activeSlot.Itemstack.StackSize > 1 && !byPlayer.IsCreative())
        {
            byPlayer.IngameError(this, IngameError.LiquidContainerOneMax, IngameError.LiquidContainerOneMax.Localize());
            return false;
        }
        if (!BannerLiquid.TryGet(activeSlot.Itemstack, blockContainer, out BannerLiquid liquidProps) || !liquidProps.IsDye)
        {
            return false;
        }
        string pattern = BannerPatternProperties.FromStack(offHandSlot.Itemstack).Type;
        if (string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        if (!liquidProps.CanTakeLiquid(activeSlot.Itemstack, blockContainer) && !byPlayer.IsCreative())
        {
            byPlayer.IngameError(this, IngameError.BannerNotEnoughDye, IngameError.BannerNotEnoughDye.Localize(liquidProps.LitresPerUse));
            return false;
        }

        if (!blockEntity.BannerProps.Patterns.TryAdd(new BannerLayer().WithPattern(pattern).WithColor(liquidProps.Color), world, byPlayer))
        {
            byPlayer.IngameError(this, IngameError.LayersLimitReached, IngameError.LayersLimitReached.Localize(Patterns.GetLayersLimit(world)));
            return false;
        }

        if (!byPlayer.IsCreative())
        {
            liquidProps.TryTakeLiquid(activeSlot.Itemstack, blockContainer);
        }

        byPlayer.DoLiquidMovedEffects(blockContainer.GetContent(activeSlot.Itemstack), 1000, BlockLiquidContainerBase.EnumLiquidDirection.Pour);
        return true;
    }

    public bool RemoveLayer(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;

        if (activeSlot?.Itemstack?.Collectible is not BlockLiquidContainerTopOpened blockContainer)
        {
            return false;
        }

        if (!blockEntity.IsEditModeEnabled(byPlayer)) return false;

        if (activeSlot.Itemstack.StackSize > 1 && !byPlayer.IsCreative())
        {
            byPlayer.IngameError(this, IngameError.LiquidContainerOneMax, IngameError.LiquidContainerOneMax.Localize());
            return false;
        }
        if (!BannerLiquid.TryGet(activeSlot.Itemstack, blockContainer, out BannerLiquid liquidProps) || !liquidProps.IsBleach)
        {
            return false;
        }
        if (!liquidProps.CanTakeLiquid(activeSlot.Itemstack, blockContainer) && !byPlayer.IsCreative())
        {
            byPlayer.IngameError(this, IngameError.BannerNotEnoughBleach, IngameError.BannerNotEnoughBleach.Localize(liquidProps.LitresPerUse));
            return false;
        }

        if (!blockEntity.BannerProps.Patterns.TryRemoveLast())
        {
            return false;
        }

        if (!byPlayer.IsCreative())
        {
            liquidProps.TryTakeLiquid(activeSlot.Itemstack, blockContainer);
        }

        byPlayer.DoLiquidMovedEffects(blockContainer.GetContent(activeSlot.Itemstack), 1000, BlockLiquidContainerBase.EnumLiquidDirection.Pour);
        return true;
    }

    public bool AddCutout(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;
        ItemSlot offHandSlot = byPlayer.Entity.LeftHandItemSlot;

        if (offHandSlot?.Itemstack?.Collectible is not ItemBannerPattern itemPattern || activeSlot?.Itemstack?.Collectible is not ItemShears)
        {
            return false;
        }

        if (!blockEntity.IsEditModeEnabled(byPlayer)) return false;

        if (!blockEntity.BannerBlock.MatchesPatternGroups(itemPattern))
        {
            byPlayer.IngameError(this, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
            return false;
        }

        string pattern = BannerPatternProperties.FromStack(offHandSlot.Itemstack).Type;
        if (string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        return blockEntity.BannerProps.Cutouts.TryAdd(new BannerLayer().WithPattern(pattern));
    }

    public bool RemoveCutout(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;
        ItemSlot offHandSlot = byPlayer.Entity.LeftHandItemSlot;
        if (!offHandSlot.Empty || activeSlot?.Itemstack?.Collectible is not ItemShears)
        {
            return false;
        }

        if (!blockEntity.IsEditModeEnabled(byPlayer)) return false;

        return blockEntity.BannerProps.Cutouts.TryRemoveLast();
    }

    public bool CopyLayers(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;

        if (activeSlot?.Itemstack?.Collectible is not BlockBanner blockBanner)
        {
            return false;
        }
        if (!blockEntity.BannerBlock.MatchesPatternGroups(blockBanner))
        {
            byPlayer.IngameError(this, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
            return false;
        }
        if (blockEntity.BannerProps.CopyTo(activeSlot.Itemstack, copyLayers: true, copyCutouts: true))
        {
            return true;
        }

        if (!blockEntity.IsEditModeEnabled(byPlayer)) return false;

        if (blockEntity.BannerProps.CopyFrom(activeSlot.Itemstack, copyLayers: true, copyCutouts: true))
        {
            return true;
        }
        byPlayer.IngameError(this, IngameError.BannerCopyLayers, IngameError.BannerCopyLayers.Localize());
        return false;
    }

    public bool Rename(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;

        if (activeSlot?.Itemstack?.Collectible is not ItemBook)
        {
            return false;
        }

        if (!blockEntity.IsEditModeEnabled(byPlayer)) return false;

        string newName = activeSlot.Itemstack.Attributes.GetString(attributeTitle);

        if (string.IsNullOrEmpty(newName))
        {
            byPlayer.IngameError(this, IngameError.BannerRename, IngameError.BannerRename.Localize());
            return false;
        }

        blockEntity.BannerProps.SetName(newName);
        return true;
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
    {
        if (world.Api is not ICoreClientAPI capi || world.BlockAccessor.GetBlockEntity(selection.Position) is not BlockEntityBanner blockEntity)
        {
            return Array.Empty<WorldInteraction>();
        }

        handling = EnumHandling.Handled;
        return BannerInteractions(blockEntity, capi, selection, forPlayer);
    }

    public WorldInteraction[] BannerInteractions(BlockEntityBanner blockEntity, ICoreClientAPI capi, BlockSelection selection, IPlayer forPlayer)
    {
        List<WorldInteraction> interactions = new List<WorldInteraction>();

        ItemStack[] bannerStacks = Array.Empty<ItemStack>();
        foreach (ItemStack stack in ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyBannerStacks))
        {
            BannerProperties stackProps = BannerProperties.FromStack(stack);
            if (blockEntity.BannerProps.Patterns.SameBaseColors(stackProps) && stackProps.Patterns.Count == 1)
            {
                bannerStacks = bannerStacks.Append(stack);
            }
        }

        interactions.Add(new WorldInteraction()
        {
            ActionLangCode = langCodeCopyLayers,
            MouseButton = EnumMouseButton.Right,
            Itemstacks = bannerStacks
        });

        if (blockEntity.IsEditModeEnabled())
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
            IRotatableBanner rotatableBanner = blockEntity.Block.GetInterface<IRotatableBanner>(capi.World, selection.Position);
            BEBehaviorWrenchOrientableBanner wrenchableBanner = blockEntity.GetBehavior<BEBehaviorWrenchOrientableBanner>();
            if (rotatableBanner != null)
            {
                interactions.AddRange(ObjectCacheUtil.TryGet<WorldInteraction[]>(capi, cacheKeyRotatableBannerInteractions));
            }
            if (wrenchableBanner != null)
            {
                interactions.Add(ObjectCacheUtil.TryGet<WorldInteraction>(capi, cacheKeyWrenchableBannerInteractions));
            }
        }

        return interactions.ToArray();
    }
}