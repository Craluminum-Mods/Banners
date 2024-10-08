﻿using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using static Flags.ConfigSystem;

namespace Flags;

public class HudElementBannerOverview : HudElement
{
    public HudElementBannerOverview(ICoreClientAPI capi) : base(capi)
    {
        capi.Event.RegisterGameTickListener(Every500ms, 500);
        ComposeHud();
        capi.Event.BlockChanged += OnBlockChanged;
        if (BannerOverviewConfig.Enabled == true)
        {
            TryOpen();
        }

        ClientSettings.Inst.AddWatcher(ModClientSetting.ShowOverview, delegate (bool on)
        {
            if (on)
            {
                TryOpen();
            }
            else
            {
                TryClose();
            }
        });
    }

    private void Every500ms(float dt)
    {
        ComposeHud();
    }

    private void ComposeHud()
    {
        if (!IsBanner())
        {
            return;
        }

        double ScaledSlotSize = GuiElementPassiveItemSlot.unscaledSlotSize.Scaled();
        double ScaledSlotPadding = GuiElementItemSlotGridBase.unscaledSlotPadding.Scaled();

        ElementBounds mainBounds = ElementStdBounds.AutosizedMainDialog
            .WithAlignment(BannerOverviewConfig.Alignment)
            .WithFixedAlignmentOffset(BannerOverviewConfig.X, BannerOverviewConfig.Y);

        ElementBounds childBounds = new ElementBounds();
        childBounds.BothSizing = ElementSizing.FitToChildren;

        ElementBounds backgroundBounds = childBounds.WithFixedPadding(ScaledSlotPadding);

        DummyInventory patterns = GetPatternsInventory();
        DummyInventory cutouts = GetCutoutsInventory();

        ElementBounds firstBounds = ElementBounds
            .FixedSize(
                fixedWidth: ScaledSlotSize * patterns.Count / 0.945.Scaled(),
                fixedHeight: ScaledSlotSize / 1.0.Scaled())
            .WithFixedPosition(
                x: 0.0,
                y: 30.0.Scaled() * 2)
            .WithAlignment(EnumDialogArea.None);

        ElementBounds secondBounds = firstBounds.CopyOffsetedSibling(
            fixedDeltaX: 0,
            fixedDeltaY: (firstBounds.fixedHeight + ScaledSlotPadding) * 2);

        if (patterns == null || patterns.Empty)
        {
            secondBounds = firstBounds.CopyOffsetedSibling().WithFixedSize(
                width: ScaledSlotSize * cutouts.Count / 0.945.Scaled(),
                height: firstBounds.fixedHeight);
        }
        else
        {
            secondBounds = secondBounds.WithFixedSize(
                width: ScaledSlotSize * cutouts.Count / 0.945.Scaled(),
                height: firstBounds.fixedHeight);
        }

        string text = langCodeGuiBannerOverviewHUDTitle.Localize();
        CairoFont titleFont = new CairoFont
        {
            Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
            Fontname = GuiStyle.StandardFontName,
            UnscaledFontsize = 18.0.Scaled()
        };
        ElementBounds titleTextBounds = ElementBounds.FixedSize(
            fixedWidth: GuiElement.scaled(text.Length) * titleFont.UnscaledFontsize,
            fixedHeight: 20.0.Scaled());

        string patternsText = langCodePatternsNoColor.Localize();
        string cutoutsText = langCodeCutoutsNoColor.Localize();

        ElementBounds patternsTextBounds = ElementBounds.Fixed(EnumDialogArea.None,
            fixedX: 0,
            fixedY: firstBounds.fixedY - GuiElement.scaled(20),
            fixedWidth: GuiElement.scaled(text.Length) * titleFont.UnscaledFontsize,
            fixedHeight: 20.0.Scaled());

        ElementBounds cutoutsTextBounds = ElementBounds.Fixed(EnumDialogArea.None,
            fixedX: 0,
            fixedY: secondBounds.fixedY - GuiElement.scaled(20),
            fixedWidth: GuiElement.scaled(text.Length) * titleFont.UnscaledFontsize,
            fixedHeight: 20.0.Scaled());

        capi.World.Api.Event.EnqueueMainThreadTask(action: () =>
        {
            Composers[guiBannerOverviewHUD] = capi.Gui.CreateCompo(guiBannerOverviewHUD, mainBounds)
            .AddDialogBG(backgroundBounds, false)
            .BeginChildElements(childBounds)
                .AddDynamicText(text, titleFont, titleTextBounds)
                .AddIf(patterns != null && !patterns.Empty)
                    .AddDynamicText(patternsText, titleFont, patternsTextBounds)
                    .AddItemSlotGrid(patterns, null, patterns.Count, firstBounds)
                .EndIf()
                .AddIf(cutouts != null && !cutouts.Empty)
                    .AddDynamicText(cutoutsText, titleFont, cutoutsTextBounds)
                    .AddItemSlotGrid(cutouts, null, cutouts.Count, secondBounds)
                .EndIf()
            .EndChildElements()
            .Compose();
        }, code: guiBannerOverviewHUD);
    }

    private DummyInventory GetPatternsInventory()
    {
        if (capi.World.Player == null)
        {
            return new DummyInventory(capi, 0);
        }

        ItemStack placedStack = GetBanner()?.BannerBlock?.OnPickBlock(capi.World, capi.World.Player?.CurrentBlockSelection?.Position);

        if (placedStack == null)
        {
            return new DummyInventory(capi, 0);
        }

        placedStack.Attributes.GetOrAddTreeAttribute(attributeBanner).RemoveAttribute(attributeCutouts);
        placedStack.TempAttributes.SetBool(attributeInBannerPreviewHUD, true);
        BannerProperties bannerProperties = BannerProperties.FromStack(placedStack);

        List<ItemStack> stacks = new();

        for (int i = bannerProperties.Patterns.Count - 1; i >= 0; i--)
        {
            if (bannerProperties.Patterns.TryRemoveLast())
            {
                ItemStack newStack = placedStack.Clone();
                newStack.Attributes.RemoveAttribute(attributeBanner);
                bannerProperties.ToStack(newStack);
                newStack.TempAttributes.SetBool(attributeInBannerPreviewHUD, true);
                stacks.Add(newStack);
            }
        }

        stacks.Reverse();
        if (stacks.Count > 0)
        {
            stacks.Add(placedStack);
        }

        DummyInventory dummyInventory = new DummyInventory(capi, stacks.Count);
        for (int i = 0; i < stacks.Count; i++)
        {
            if (stacks[i] == null)
            {
                continue;
            }
            dummyInventory[i].Itemstack = stacks[i];
        }
        return dummyInventory;
    }

    private DummyInventory GetCutoutsInventory()
    {
        if (capi.World.Player == null)
        {
            return new DummyInventory(capi, 0);
        }

        ItemStack placedStack = GetBanner()?.BannerBlock?.OnPickBlock(capi.World, capi.World.Player?.CurrentBlockSelection?.Position);

        if (placedStack == null)
        {
            return new DummyInventory(capi, 0);
        }

        placedStack.TempAttributes.SetBool(attributeInBannerPreviewHUD, true);
        BannerProperties bannerProperties = BannerProperties.FromStack(placedStack);

        placedStack.Attributes.RemoveAttribute(attributeBanner);
        bannerProperties.ToStack(placedStack);

        List<ItemStack> stacks = new();

        for (int i = bannerProperties.Cutouts.Count - 1; i >= 0; i--)
        {
            if (bannerProperties.Cutouts.TryRemoveLast())
            {
                ItemStack newStack = placedStack.Clone();
                newStack.Attributes.RemoveAttribute(attributeBanner);
                bannerProperties.ToStack(newStack);
                newStack.TempAttributes.SetBool(attributeInBannerPreviewHUD, true);
                stacks.Add(newStack);
            }
        }

        stacks.Reverse();
        if (stacks.Any())
        {
            stacks.Add(placedStack);
        }

        DummyInventory dummyInventory = new DummyInventory(capi, stacks.Count);
        for (int i = 0; i < stacks.Count; i++)
        {
            if (stacks[i] == null)
            {
                continue;
            }
            dummyInventory[i].Itemstack = stacks[i];
        }
        return dummyInventory;
    }

    private BlockEntityBanner GetBanner()
    {
        return capi.World.Player?.CurrentBlockSelection == null
            ? null
            : capi.World.BlockAccessor.GetBlockEntity<BlockEntityBanner>(capi.World.Player.CurrentBlockSelection.Position);
    }

    private bool IsBanner()
    {
        return capi.World.Player?.CurrentBlockSelection?.Block is BlockBanner;
    }

    public override bool ShouldReceiveRenderEvents()
    {
        if (!IsBanner())
        {
            return false;
        }

        BlockEntityBanner banner = GetBanner();
        return banner != null && (banner.BannerProps.Patterns.Count > 1 || banner.BannerProps.Cutouts.Any);
    }

    private void OnBlockChanged(BlockPos pos, Block oldBlock)
    {
        IPlayer player = capi.World.Player;
        if (player?.CurrentBlockSelection != null && pos.Equals(player.CurrentBlockSelection.Position))
        {
            ComposeHud();
        }
    }

    public override bool ShouldReceiveKeyboardEvents() => false;
    public override bool ShouldReceiveMouseEvents() => false;

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();
        BannerOverviewConfig.Enabled = true;
    }

    public override void OnGuiClosed()
    {
        base.OnGuiClosed();
        BannerOverviewConfig.Enabled = false;
    }
}
