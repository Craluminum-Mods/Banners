using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
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

        if (blockEntity.BannerProps.Modes[BannerMode.PickUp_On] && PickUp(byPlayer))
        {
            handling = EnumHandling.PreventDefault;
            return true;
        }

        ItemSlot leftSlot = byPlayer.Entity.LeftHandItemSlot;
        ItemSlot rightSlot = byPlayer.Entity.RightHandItemSlot;

        if (AddLayer(byPlayer, leftSlot, rightSlot, blockEntity.BannerProps, blockEntity.BannerBlock)
            || RemoveLayer(byPlayer, rightSlot, blockEntity.BannerProps, blockEntity.BannerBlock)
            || AddCutout(byPlayer, leftSlot, rightSlot, blockEntity.BannerProps, blockEntity.BannerBlock)
            || RemoveCutout(byPlayer, leftSlot, rightSlot, blockEntity.BannerProps)
            || CopyLayers(byPlayer, rightSlot, blockEntity.BannerProps, blockEntity.BannerBlock)
            || Rename(byPlayer, rightSlot, blockEntity.BannerProps, blockEntity.BannerBlock))
        {
            blockEntity.MarkDirty(true);
            byPlayer.Entity.RightHandItemSlot.MarkDirty();
            byPlayer.Entity.LeftHandItemSlot.MarkDirty();
            handling = EnumHandling.PreventDefault;
            return true;
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
    }

    public bool PickUp(IPlayer byPlayer)
    {
        IWorldAccessor world = byPlayer.Entity.World;
        BlockSelection blockSel = byPlayer.CurrentBlockSelection;
        ItemStack[] dropStacks = new ItemStack[1] { blockSel.Block.OnPickBlock(world, blockSel.Position) };
        ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        bool heldSlotSuitable = activeSlot.Empty || (dropStacks.Length >= 1 && activeSlot.Itemstack.Equals(world, dropStacks[0], GlobalConstants.IgnoredStackAttributes));
        if (!heldSlotSuitable || !world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
        {
            return false;
        }
        if (byPlayer.Entity.Controls.ShiftKey)
        {
            return false;
        }
        if (world.Side != EnumAppSide.Server || !BlockBehaviorReinforcable.AllowRightClickPickup(world, blockSel.Position, byPlayer))
        {
            return true;
        }
        bool blockToBreak = true;
        foreach (ItemStack stack in dropStacks)
        {
            ItemStack origStack = stack.Clone();
            if (!byPlayer.InventoryManager.TryGiveItemstack(stack, slotNotifyEffect: true))
            {
                world.SpawnItemEntity(stack, blockSel.Position.ToVec3d().AddCopy(0.5, 0.1, 0.5));
            }
            TreeAttribute tree = new TreeAttribute();
            tree["itemstack"] = new ItemstackAttribute(origStack.Clone());
            tree["byentityid"] = new LongAttribute(byPlayer.Entity.EntityId);
            world.Api.Event.PushEvent("onitemcollected", tree);
            if (blockToBreak)
            {
                blockToBreak = false;
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.BlockAccessor.TriggerNeighbourBlockUpdate(blockSel.Position);
            }
            world.PlaySoundAt(blockSel.Block.GetSounds(world.BlockAccessor, blockSel).Place, byPlayer);
        }
        return true;
    }

    public bool AddLayer(IPlayer byPlayer, ItemSlot leftSlot, ItemSlot rightSlot, BannerProperties bannerProps, BlockBanner blockBanner, bool isPreview = false)
    {
        if (leftSlot?.Itemstack?.Collectible is not ItemBannerPattern itemPattern
            || rightSlot?.Itemstack?.Collectible is not BlockLiquidContainerTopOpened blockContainer
            || !blockBanner.IsEditModeEnabled(bannerProps, byPlayer, printError: !isPreview))
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

    public bool RemoveLayer(IPlayer byPlayer, ItemSlot rightSlot, BannerProperties bannerProps, BlockBanner blockBanner, bool isPreview = false)
    {
        if (rightSlot?.Itemstack?.Collectible is not BlockLiquidContainerTopOpened blockContainer
            || !blockBanner.IsEditModeEnabled(bannerProps, byPlayer, printError: !isPreview))
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

    public bool AddCutout(IPlayer byPlayer, ItemSlot leftSlot, ItemSlot rightSlot, BannerProperties bannerProps, BlockBanner blockBanner, bool isPreview = false)
    {
        CollectibleBehaviorCutoutTool cutoutToolBehavior = rightSlot?.Itemstack?.Collectible?.GetBehavior<CollectibleBehaviorCutoutTool>();
        if (leftSlot?.Itemstack?.Collectible is not ItemBannerPattern itemPattern || cutoutToolBehavior == null)
        {
            return false;
        }

        if (!blockBanner.IsEditModeEnabled(bannerProps, byPlayer, printError: !isPreview)) return false;

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

        bool applied = cutoutToolBehavior.HasEnoughDurability(rightSlot) && bannerProps.Cutouts.TryAdd(new BannerLayer().WithPattern(patternProperties.Type));

        if (applied && !isPreview)
        {
            cutoutToolBehavior.ConsumeDurability(byPlayer.Entity.World, byPlayer.Entity, rightSlot);
        }
        if (applied && !isPreview) cutoutToolBehavior.PlayCutSound(byPlayer);
        return applied;
    }

    public bool RemoveCutout(IPlayer byPlayer, ItemSlot leftSlot, ItemSlot rightSlot, BannerProperties bannerProps, bool isPreview = false)
    {
        CollectibleBehaviorCutoutTool cutoutToolBehavior = rightSlot?.Itemstack?.Collectible?.GetBehavior<CollectibleBehaviorCutoutTool>();
        if (!leftSlot.Empty || cutoutToolBehavior == null)
        {
            return false;
        }

        if (!blockBanner.IsEditModeEnabled(bannerProps, byPlayer, printError: !isPreview)) return false;

        bool applied = bannerProps.Cutouts.TryRemoveLast();
        if (applied && !isPreview) cutoutToolBehavior.PlayRepairSound(byPlayer);
        return applied;
    }

    public bool CopyLayers(IPlayer byPlayer, ItemSlot rightSlot, BannerProperties bannerProps, BlockBanner blockBanner, bool isPreview = false)
    {
        if (rightSlot?.Itemstack?.Collectible is not BlockBanner anotherBlockBanner)
        {
            return false;
        }

        if (!blockBanner.MatchesPatternGroups(anotherBlockBanner))
        {
            if (!isPreview) byPlayer.IngameError(blockBanner, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
            return false;
        }

        if (bannerProps.CopyTo(rightSlot.Itemstack, copyLayers: true, copyCutouts: true))
        {
            return true;
        }

        if (!blockBanner.IsEditModeEnabled(bannerProps, byPlayer, printError: !isPreview)) return false;

        if (bannerProps.CopyFrom(rightSlot.Itemstack, copyLayers: true, copyCutouts: true))
        {
            return true;
        }
        if (!isPreview) byPlayer.IngameError(blockBanner, IngameError.BannerCopyLayers, IngameError.BannerCopyLayers.Localize());
        return false;
    }

    public bool Rename(IPlayer byPlayer, ItemSlot rightSlot, BannerProperties bannerProps, BlockBanner blockBanner)
    {
        CollectibleBehaviorRenameTool renameToolBehavior = rightSlot?.Itemstack?.Collectible?.GetBehavior<CollectibleBehaviorRenameTool>();
        if (renameToolBehavior == null || !blockBanner.IsEditModeEnabled(bannerProps, byPlayer))
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
        return BannerInteractions(blockEntity.BannerProps, capi, blockSel);
    }

    public WorldInteraction[] BannerInteractions(BannerProperties bannerProps, ICoreClientAPI capi, BlockSelection blockSel)
    {
        List<WorldInteraction> interactions = new List<WorldInteraction>();

        ItemStack[] bannerStacks = Array.Empty<ItemStack>();
        foreach (ItemStack stack in ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyBannerStacks))
        {
            BannerProperties stackProps = BannerProperties.FromStack(stack);
            if (bannerProps.Patterns.SameBaseColors(stackProps) && stackProps.Patterns.Count == 1)
            {
                bannerStacks = bannerStacks.Append(stack);
            }
        }

        if (bannerProps.Modes[BannerMode.PickUp_On])
        {
            interactions.Add(new WorldInteraction
            {
                ActionLangCode = langCodeRightClickPickUp,
                MouseButton = EnumMouseButton.Right,
                RequireFreeHand = true
            });
        }

        interactions.Add(new WorldInteraction()
        {
            ActionLangCode = blockBanner.IsEditModeEnabled(bannerProps, capi, printError: false) ? langCodeCopyLayers : langCodeCopyLayersFromPlaced,
            MouseButton = EnumMouseButton.Right,
            Itemstacks = bannerStacks
        });

        if (blockBanner.IsEditModeEnabled(bannerProps, capi, printError: false))
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

        if (AddLayer(player, leftSlot, rightSlot, placedProps, blockEntity.BannerBlock, isPreview: true)
            || RemoveLayer(player, rightSlot, placedProps, blockEntity.BannerBlock, isPreview: true)
            || AddCutout(player, leftSlot, rightSlot, placedProps, blockEntity.BannerBlock, isPreview: true)
            || RemoveCutout(player, leftSlot, rightSlot, placedProps, isPreview: true)
            || CopyLayers(player, rightSlot, placedProps, blockEntity.BannerBlock, isPreview: true))
        {
            placedStack.Attributes.RemoveAttribute(attributeBanner);
            placedProps.ToStack(placedStack);
            return placedStack;
        }
        return null;
    }
}