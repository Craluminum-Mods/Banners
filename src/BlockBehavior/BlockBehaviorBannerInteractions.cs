using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

public class BlockBehaviorBannerInteractions : BlockBehavior
{
    public List<ItemStack> DyeStacks { get; protected set; } = new();
    public List<ItemStack> BleachStacks { get; protected set; } = new();
    public List<ItemStack> BookStacks { get; protected set; } = new();
    public List<ItemStack> WrenchStacks { get; protected set; } = new();

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

        if (activeSlot?.Itemstack?.Collectible is not BlockLiquidContainerTopOpened blockContainer)
        {
            return false;
        }
        if (offHandSlot?.Itemstack?.Collectible is not ItemBannerPattern itemPattern)
        {
            return false;
        }
        if (!BannerLiquid.TryGet(activeSlot.Itemstack, blockContainer, out BannerLiquid liquidProps) || !liquidProps.IsDye)
        {
            return false;
        }
        if (!blockEntity.BannerProps.AddLayer(world, byPlayer, new BannerLayer(BannerPatternProperties.FromStack(offHandSlot.Itemstack, itemPattern).Type, liquidProps)))
        {
            return false;
        }

        liquidProps.TryTakeLiquid(activeSlot.Itemstack, blockContainer);
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
        if (blockEntity.BannerProps.RemoveLastLayer())
        {
            liquidProps.TryTakeLiquid(activeSlot.Itemstack, blockContainer);
            return true;
        }

        return false;
    }

    public bool CopyLayers(IPlayer byPlayer, BlockEntityBanner blockEntity)
    {
        ItemSlot activeSlot = byPlayer.Entity.RightHandItemSlot;

        if (activeSlot?.Itemstack?.Collectible is not BlockBanner)
        {
            return false;
        }
        if (blockEntity.BannerProps.CopyFrom(activeSlot.Itemstack, copyLayers: true))
        {
            return true;
        }
        if (blockEntity.BannerProps.CopyTo(activeSlot.Itemstack, copyLayers: true))
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

        string newName = activeSlot.Itemstack.Attributes.GetString("title");

        if (string.IsNullOrEmpty(newName))
        {
            (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(this, IngameError.BannerRename, IngameError.BannerRename.Localize());
            return false;
        }

        blockEntity.BannerProps.SetName(newName);
        return true;
    }

    public override void OnLoaded(ICoreAPI api)
    {
        if (api is not ICoreClientAPI capi)
        {
            return;
        }

        foreach (CollectibleObject obj in api.World.Collectibles)
        {
            if (obj is ItemBook)
            {
                BookStacks.AddRange(obj.GetHandBookStacks(capi) ?? Array.Empty<ItemStack>().ToList());
            }
            if (obj is ItemWrench)
            {
                WrenchStacks.AddRange(obj.GetHandBookStacks(capi) ?? Array.Empty<ItemStack>().ToList());
            }
            if (BannerLiquid.TryGet(obj, out BannerLiquid liquidProps))
            {
                foreach (ItemStack stack in obj.GetHandBookStacks(capi) ?? Array.Empty<ItemStack>().ToList())
                {
                    switch (liquidProps.Type)
                    {
                        case EnumBannerLiquid.Dye: DyeStacks.Add(stack); break;
                        case EnumBannerLiquid.Bleach: BleachStacks.Add(stack); break;
                    };
                }
            }
        }
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        if (api is ICoreClientAPI)
        {
            DyeStacks.Clear();
            BleachStacks.Clear();
            BookStacks.Clear();
            WrenchStacks.Clear();
        }
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
        WorldInteraction[] interactions = Array.Empty<WorldInteraction>();

        List<ItemStack> bannerStacks = new();
        foreach (ItemStack stack in capi.World.Collectibles.Where(obj => obj is BlockBanner).SelectMany(obj => obj.GetHandBookStacks(capi) ?? Array.Empty<ItemStack>().ToList()))
        {
            ItemStack newStack = stack.Clone();
            newStack.StackSize = 1;
            BannerProperties placedProps = blockEntity.BannerProps;
            BannerProperties stackProps = BannerProperties.FromStack(stack);
            if (placedProps.BaseColor == stackProps.BaseColor && stackProps.Layers.Count == 1)
            {
                bannerStacks.Add(newStack);
            }
        }

        interactions = interactions.Concat(new WorldInteraction[]
        {
                new()
                {
                    ActionLangCode = langCodeCopyLayers,
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = bannerStacks.ToArray()
                }
        }).ToArray();

        interactions = interactions.Concat(new WorldInteraction[]
        {
                new WorldInteraction()
                {
                    ActionLangCode = langCodeAddLayer,
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = DyeStacks.ToArray()
                },
                new WorldInteraction()
                {
                    ActionLangCode =  langCodeRemovelayer,
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = BleachStacks.ToArray()
                },
                new WorldInteraction()
                {
                    ActionLangCode = langCodeRename,
                    MouseButton = EnumMouseButton.Right,
                    Itemstacks = BookStacks.ToArray()
                }
        }).ToArray();

        IRotatableBanner rotatableBanner = blockEntity.Block.GetInterface<IRotatableBanner>(capi.World, selection.Position);
        BEBehaviorWrenchOrientableBanner wrenchableBanner = blockEntity.GetBehavior<BEBehaviorWrenchOrientableBanner>();

        if (rotatableBanner != null)
        {
            interactions = interactions.Concat(rotatableBanner.GetPlacedBlockInteractionHelp(capi.World, selection, forPlayer, WrenchStacks)).ToArray();
        }
        if (wrenchableBanner != null)
        {
            interactions = interactions.Concat(wrenchableBanner.GetPlacedBlockInteractionHelp(capi.World, selection, forPlayer, WrenchStacks)).ToArray();
        }

        return interactions;
    }
}