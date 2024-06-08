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

        if (AddLayer(world, byPlayer, blockEntity) || RemoveLayer(byPlayer, blockEntity) || CopyLayers(byPlayer, blockEntity) || Rename(byPlayer, blockEntity))
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
        if (!itemPattern.PatternGroups.Any(groups1 => blockEntity.BannerBlock.PatternGroups.Any(groups2 => groups1 == groups2)))
        {
            (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
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
            (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, IngameError.BannerNotEnoughDye, IngameError.BannerNotEnoughDye.Localize(liquidProps.LitresPerUse));
            return false;
        }

        if (!blockEntity.BannerProps.AddLayer(new BannerLayer(pattern, liquidProps), world, byPlayer))
        {
            (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, IngameError.LayersLimitReached, IngameError.LayersLimitReached.Localize(BannerProperties.GetLayersLimit(world)));
            return false;
        }

        if (!byPlayer.IsCreative())
        {
            liquidProps.TryTakeLiquid(activeSlot.Itemstack, blockContainer);
        }
        return true;
    }

    public bool RemoveLayer(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;

        if (activeSlot?.Itemstack?.Collectible is not BlockLiquidContainerTopOpened blockContainer)
        {
            return false;
        }
        if (!BannerLiquid.TryGet(activeSlot.Itemstack, blockContainer, out BannerLiquid liquidProps) || !liquidProps.IsBleach)
        {
            return false;
        }

        if (!liquidProps.CanTakeLiquid(activeSlot.Itemstack, blockContainer) && !byPlayer.IsCreative())
        {
            (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, IngameError.BannerNotEnoughBleach, IngameError.BannerNotEnoughBleach.Localize(liquidProps.LitresPerUse));
            return false;
        }

        if (!blockEntity.BannerProps.RemoveLastLayer())
        {
            return false;
        }

        if (!byPlayer.IsCreative())
        {
            liquidProps.TryTakeLiquid(activeSlot.Itemstack, blockContainer);
        }
        return true;
    }

    public bool CopyLayers(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;

        if (activeSlot?.Itemstack?.Collectible is not BlockBanner blockBanner)
        {
            return false;
        }
        if (!blockBanner.PatternGroups.Any(groups1 => blockEntity.BannerBlock.PatternGroups.Any(groups2 => groups1 == groups2)))
        {
            (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, IngameError.BannerPatternGroups, IngameError.BannerPatternGroups.Localize());
            return false;
        }
        if (blockEntity.BannerProps.CopyFrom(activeSlot.Itemstack, copyLayers: true) || blockEntity.BannerProps.CopyTo(activeSlot.Itemstack, copyLayers: true))
        {
            return true;
        }
        (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, IngameError.BannerCopyLayers, IngameError.BannerCopyLayers.Localize());
        return false;
    }

    public bool Rename(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;

        if (activeSlot?.Itemstack?.Collectible is not ItemBook)
        {
            return false;
        }

        string newName = activeSlot.Itemstack.Attributes.GetString(attributeTitle);

        if (string.IsNullOrEmpty(newName))
        {
            (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, IngameError.BannerRename, IngameError.BannerRename.Localize());
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
            if (blockEntity.BannerProps.SameBaseColors(stackProps) && stackProps.Layers.Count == 1)
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

        interactions.AddRange(new List<WorldInteraction>()
        {
            new WorldInteraction()
            {
                ActionLangCode = langCodeAddLayer,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyDyeStacks)
            },
            new WorldInteraction()
            {
                ActionLangCode =  langCodeRemovelayer,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyBleachStacks)
            },
            new WorldInteraction()
            {
                ActionLangCode = langCodeRename,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyBookStacks)
            }
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
        return interactions.ToArray();
    }
}