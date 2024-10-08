﻿using Cairo;
using System;
using System.Linq;
using Vintagestory.API.Client;
using static Flags.ConfigSystem;
using static Flags.GuiElementExtensions;

namespace Flags;

public class GuiDialogBannerConfig : GuiDialog
{
    public override string ToggleKeyCombinationCode => guiBannerConfigDialog;

    public GuiDialogBannerConfig(ICoreClientAPI capi) : base(capi)
    {
        ComposeDialog();
    }

    private void ComposeDialog()
    {
        SingleComposer?.Dispose();

        string[] alignNames = Enum.GetNames(typeof(EnumDialogArea)).Select(x => $"{langCodeDialogArea}{x}".Localize()).ToArray();
        string[] alignValues = Enum.GetValues<EnumDialogArea>().Select(x => x.ToString()).ToArray();

        double indent = GuiElement.scaled(30);
        double gap = GuiElement.scaled(10);
        double offsetY = indent + gap;

        string textAlign = langCodeAlignment.Localize();
        string textX = langCodePosX.Localize();
        string textY = langCodePosY.Localize();

        CairoFont textFont = CairoFont.WhiteSmallText().WithFontSize((float)gap * 2);

        CairoFont titleFont = CairoFont.WhiteSmallishText().Clone().WithWeight(FontWeight.Bold);
        titleFont.Color[3] = 0.6;

        ElementBounds mainBounds = ElementStdBounds.AutosizedMainDialog
            .WithAlignment(EnumDialogArea.LeftMiddle)
            .WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, GuiStyle.DialogToScreenPadding);

        ElementBounds childBounds = new ElementBounds().WithSizing(ElementSizing.FitToChildren);
        ElementBounds backgroundBounds = childBounds.WithFixedPadding(GuiElement.scaled(15));

        ElementBounds leftBounds = ElementBounds.FixedSize(indent * 10, indent).WithFixedOffset(0, indent);
        ElementBounds rightBounds = leftBounds.RightCopy(gap);

        GuiComposer composer = capi.Gui.CreateCompo(guiBannerConfigDialog, mainBounds)
        .AddDialogBG(backgroundBounds, false)
        .AddDialogTitleBarWithBg(langCodeGuiBannerConfigDialogTitle.Localize(), () => TryClose())
        .BeginChildElements(childBounds)
            .AddDynamicText("", titleFont, leftBounds, "textTitlePreview")
            .AddDynamicText(textAlign, textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap))
            .AddDropDown(alignValues, alignNames, 0, OnDropdownPreview, BelowCopySet(ref rightBounds, fixedDeltaY: gap), "dropdownPreviewAlign")
            .AddDynamicText(textX, textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap))
            .AddSlider(OnSliderPreviewX, BelowCopySet(ref rightBounds, fixedDeltaY: gap), "sliderPreviewX")
            .AddDynamicText(textY, textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap))
            .AddSlider(OnSliderPreviewY, BelowCopySet(ref rightBounds, fixedDeltaY: gap), "sliderPreviewY")

            .AddDynamicText("", titleFont, BelowCopySet(ref leftBounds, fixedDeltaY: offsetY), "textTitleOverview")
            .AddDynamicText(textAlign, textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap), "textOverviewAlign")
            .AddDropDown(alignValues, alignNames, 0, OnDropdownOverview, BelowCopySet(ref rightBounds, fixedDeltaY: offsetY * 2), "dropdownOverviewAlign")
            .AddDynamicText(textX, textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap))
            .AddSlider(OnSliderOverviewX, BelowCopySet(ref rightBounds, fixedDeltaY: gap), "sliderOverviewX")
            .AddDynamicText(textY, textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap))
            .AddSlider(OnSliderOverviewY, BelowCopySet(ref rightBounds, fixedDeltaY: gap), "sliderOverviewY")

            .AddDynamicText("", titleFont, BelowCopySet(ref leftBounds, fixedDeltaY: offsetY), "textTitleOther")
            .AddDynamicText("", textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap), "textShowPreview")
            .AddSwitch(OnTooglePreview, BelowCopySet(ref rightBounds, fixedDeltaY: offsetY * 2), "switchPreview")
            .AddDynamicText("", textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap), "textShowOverview")
            .AddSwitch(OnToogleOverview, BelowCopySet(ref rightBounds, fixedDeltaY: gap), "switchOverview")
            .AddDynamicText("", textFont, BelowCopySet(ref leftBounds, fixedDeltaY: gap), "textShowExtraInfo")
            .AddSwitch(OnToogleExtraInfo, BelowCopySet(ref rightBounds, fixedDeltaY: gap), "switchExtraInfo")
        .EndChildElements();
        SingleComposer = composer.Compose();

        SingleComposer?.GetDropDown("dropdownPreviewAlign")?.SetSelectedIndex((int)BannerPreviewConfig.Alignment);
        SingleComposer?.GetDropDown("dropdownOverviewAlign")?.SetSelectedIndex((int)BannerOverviewConfig.Alignment);
        SingleComposer?.GetDynamicText("textTitlePreview")?.SetNewText(guiBannerPreviewHUD.Localize());
        SingleComposer?.GetDynamicText("textTitleOverview")?.SetNewText(guiBannerOverviewHUD.Localize());
        SingleComposer?.GetDynamicText("textTitleOther")?.SetNewText(langCodeOther.Localize());
        SingleComposer?.GetDynamicText("textShowPreview")?.SetNewText(langCodeGuiBannerPreviewHUDTitle.Localize());
        SingleComposer?.GetDynamicText("textShowOverview")?.SetNewText(langCodeGuiBannerOverviewHUDTitle.Localize());
        SingleComposer?.GetDynamicText("textShowExtraInfo")?.SetNewText(langCodeBannerExtraInfo.Localize());
        SingleComposer?.GetSwitch("switchPreview")?.SetValue(BannerPreviewConfig.Enabled);
        SingleComposer?.GetSwitch("switchOverview")?.SetValue(BannerOverviewConfig.Enabled);
        SingleComposer?.GetSwitch("switchExtraInfo")?.SetValue(BannerExtraInfoConfig.Enabled);
        SingleComposer?.GetSlider("sliderPreviewX")?.SetValues((int)BannerPreviewConfig.X, -100, 100, 1);
        SingleComposer?.GetSlider("sliderPreviewY")?.SetValues((int)BannerPreviewConfig.Y, -100, 100, 1);
        SingleComposer?.GetSlider("sliderOverviewX")?.SetValues((int)BannerOverviewConfig.X, -100, 100, 1);
        SingleComposer?.GetSlider("sliderOverviewY")?.SetValues((int)BannerOverviewConfig.Y, -100, 100, 1);
    }

    private void OnTooglePreview(bool newValue) => BannerPreviewConfig.Enabled = newValue;
    private void OnToogleOverview(bool newValue) => BannerOverviewConfig.Enabled = newValue;
    private void OnToogleExtraInfo(bool newValue) => BannerExtraInfoConfig.Enabled = newValue;

    private void OnDropdownPreview(string code, bool selected)
    {
        EnumDialogArea newButton = BannerPreviewConfig.Alignment;
        if (Enum.TryParse(code, out newButton))
        {
            BannerPreviewConfig.Alignment = newButton;
        }
    }

    private void OnDropdownOverview(string code, bool selected)
    {
        EnumDialogArea newButton = BannerOverviewConfig.Alignment;
        if (Enum.TryParse(code, out newButton))
        {
            BannerOverviewConfig.Alignment = newButton;
        }
    }

    private bool OnSliderPreviewX(int newValue)
    {
        BannerPreviewConfig.X = newValue;
        return true;
    }

    private bool OnSliderPreviewY(int newValue)
    {
        BannerPreviewConfig.Y = newValue;
        return true;
    }

    private bool OnSliderOverviewX(int newValue)
    {
        BannerOverviewConfig.X = newValue;
        return true;
    }

    private bool OnSliderOverviewY(int newValue)
    {
        BannerOverviewConfig.Y = newValue; 
        return true;
    }

    public override bool TryOpen()
    {
        ComposeDialog();
        return base.TryOpen();
    }
}
