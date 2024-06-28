using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace Flags;

public class HudElementBannerOverview : HudElement
{
    public override string ToggleKeyCombinationCode => ModHotkey.BannerOverviewHud;

    public HudElementBannerOverview(ICoreClientAPI capi) : base(capi)
    {
        capi.Event.RegisterGameTickListener(Every500ms, 500);
        capi.Event.BlockChanged += OnBlockChanged;
        ComposeHud();
        if (Hotkeys.ShowBannerOverviewHud == true)
        {
            TryOpen();
        }

        ClientSettings.Inst.AddWatcher(ModClientSetting.ShowBannerOverviewHud, delegate (bool on)
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

    private void ComposeHud()
    {
        double ScaledSlotSize = GuiElementPassiveItemSlot.unscaledSlotSize.Scaled();
        double ScaledSlotPadding = GuiElementItemSlotGridBase.unscaledSlotPadding.Scaled();

        ElementBounds mainBounds = ElementStdBounds.AutosizedMainDialog
            .WithAlignment(EnumDialogArea.LeftTop)
            .WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, GuiStyle.DialogToScreenPadding);

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

        ElementBounds secondBounds = firstBounds
            .CopyOffsetedSibling(
                fixedDeltaX: 0,
                fixedDeltaY: (firstBounds.fixedHeight + ScaledSlotPadding) * 2)
            .WithFixedSize(
                width: ScaledSlotSize * cutouts.Count / 0.945.Scaled(),
                height: firstBounds.fixedHeight);

        string text = guiBannerOverviewHUD.Localize();
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

        Composers[guiBannerOverviewHUD] = capi.Gui.CreateCompo(guiBannerOverviewHUD, mainBounds)
        .AddDialogBG(backgroundBounds, false)
        .BeginChildElements(childBounds)
        .AddDynamicText(text, titleFont, titleTextBounds)
        .AddIf(patterns != null)
            .AddDynamicText(patternsText, titleFont, patternsTextBounds)
            .AddItemSlotGrid(patterns, null, patterns.Count, firstBounds)
        .EndIf()
        .AddIf(cutouts != null && cutouts.Count != 0)
            .AddDynamicText(cutoutsText, titleFont, cutoutsTextBounds)
            .AddItemSlotGrid(cutouts, null, cutouts.Count, secondBounds)
        .EndIf()
        .EndChildElements()
        .Compose();
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

    public override bool ShouldReceiveRenderEvents()
    {
        BlockEntityBanner banner = GetBanner();
        return banner != null && (banner.BannerProps.Patterns.Count > 1 || banner.BannerProps.Cutouts.Any);
    }

    public override bool ShouldReceiveKeyboardEvents() => false;
    public override bool ShouldReceiveMouseEvents() => false;

    private void OnBlockChanged(BlockPos pos, Block oldBlock)
    {
        if (capi.World.Player?.CurrentBlockSelection != null && pos.Equals(capi.World.Player.CurrentBlockSelection.Position))
        {
            ComposeHud();
        }
        else
        {
            ClearComposers();
        }
    }

    private void Every500ms(float dt) => ComposeHud();

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();
        Hotkeys.ShowBannerOverviewHud = true;
    }

    public override void OnGuiClosed()
    {
        base.OnGuiClosed();
        Hotkeys.ShowBannerOverviewHud = false;
    }
}
