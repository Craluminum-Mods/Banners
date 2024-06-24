using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace Flags;

public class HudElementBannerPreview : HudElement
{
    public override string ToggleKeyCombinationCode => ModHotkey.BannerPreviewHud;

    public HudElementBannerPreview(ICoreClientAPI capi) : base(capi)
    {
        capi.Event.RegisterGameTickListener(Every500ms, 500);
        capi.Event.BlockChanged += OnBlockChanged;
        ComposeBannerPreviewHud();
        if (Hotkeys.ShowBannerPreviewHud == true)
        {
            TryOpen();
        }

        ClientSettings.Inst.AddWatcher(ModClientSetting.ShowBannerPreviewHud, delegate (bool on)
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

    private void ComposeBannerPreviewHud()
    {
        ElementBounds mainBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.LeftMiddle).WithFixedAlignmentOffset(50.0, GuiStyle.DialogToScreenPadding);
        ElementBounds childBounds = new ElementBounds();
        childBounds.BothSizing = ElementSizing.FitToChildren;

        double gap = GuiElement.scaled(GuiStyle.NormalFontSize);
        ElementBounds backgroundBounds = childBounds.WithFixedPadding(GuiElement.scaled(15));

        ElementBounds leftBounds = ElementBounds.Fixed(EnumDialogArea.None, 0.0, GuiElement.scaled(40.0), GuiElement.scaled(300.0), GuiElement.scaled(400.0));
        ElementBounds leftItemBounds = leftBounds.FlatCopy().WithFixedOffset(0, GuiElement.scaled(50));

        ElementBounds rightBounds = ElementBounds.Fixed(EnumDialogArea.None, GuiElement.scaled(315.0) , GuiElement.scaled(40.0), GuiElement.scaled(300.0), GuiElement.scaled(400.0));
        ElementBounds rightItemBounds = rightBounds.CopyOffsetedSibling(0, GuiElement.scaled(50));

        CairoFont titleFont = new CairoFont
        {
            Color = (double[])GuiStyle.DialogDefaultTextColor.Clone(),
            Fontname = GuiStyle.StandardFontName,
            UnscaledFontsize = GuiElement.scaled(GuiStyle.NormalFontSize),
            Orientation = EnumTextOrientation.Center
        };
        ElementBounds titleTextBounds = ElementBounds.Fixed(EnumDialogArea.None, 0.0, GuiElement.scaled(-10.0), (leftBounds.fixedWidth * 2) + GuiElement.scaled(5), GuiElement.scaled(35));

        double iconSize = GuiElement.scaled(64);
        double[] iconColor = ColorUtil.Hex2Doubles(HexColor.SchematicTransparent);
        double[] strokeColor = GuiStyle.HotbarNumberTextColor;
        double iconStrokeSize = 1;
        ElementBounds iconBounds = ElementBounds.FixedPos(EnumDialogArea.CenterMiddle, leftBounds.fixedX / 2, leftBounds.fixedY / 2).FixedGrow(iconSize);
        ElementBounds iconBackgroundBounds = iconBounds.CopyOffsetedSibling().WithFixedPadding(15);

        RichTextComponentBase[] leftStack = GetPreviousBannerForPreview((double)GuiElement.scaled(300));
        RichTextComponentBase[] rightStack = GetNextBannerForPreview((double)GuiElement.scaled(300));

        Composers[guiBannerPreviewHUD] = capi.Gui.CreateCompo(guiBannerPreviewHUD, mainBounds)
            .AddDialogBG(backgroundBounds, false)
            .BeginChildElements(childBounds)
                .AddDynamicText(guiBannerPreviewHUD.Localize(), titleFont, titleTextBounds)
                .AddGameOverlay(leftBounds)
                .AddInset(leftBounds, 6)
                .AddIf(leftStack != null)
                    .AddRichtext(leftStack, leftItemBounds)
                .EndIf()
                .AddGameOverlay(rightBounds)
                .AddInset(rightBounds, 6)
                .AddIf(rightStack != null)
                    .AddRichtext(rightStack, rightItemBounds)
                .EndIf()
                .AddIf(leftStack != null && rightStack != null)
                    .AddDynamicCustomDraw(iconBounds, (ctx, surface, bounds) => ctx.DrawChevron(surface, bounds, iconSize, iconColor, strokeColor, iconStrokeSize))
                .EndIf()
            .EndChildElements()
        .Compose();
    }

    private RichTextComponentBase[] GetPreviousBannerForPreview(double size)
    {
        if (capi.World.Player == null)
        {
            return null;
        }

        ItemStack placedStack = GetBanner()?.BannerBlock?.OnPickBlock(capi.World, capi.World.Player?.CurrentBlockSelection?.Position);
        placedStack?.TempAttributes.SetBool(attributeInBannerPreviewHUD, true);
        return placedStack switch
        {
            null => null,
            _ => new RichTextComponentBase[] { new ItemstackTextComponent(capi, placedStack, size) },
        };
    }

    private RichTextComponentBase[] GetNextBannerForPreview(double size)
    {
        if (capi.World.Player == null)
        {
            return null;
        }

        ItemStack nextStack = BlockBehaviorBannerInteractions.GetPreview(capi.World.Player);
        nextStack?.TempAttributes.SetBool(attributeInBannerPreviewHUD, true);
        return nextStack switch
        {
            null => null,
            _ => new RichTextComponentBase[] { new ItemstackTextComponent(capi, nextStack, size) },
        };
    }

    private BlockEntityBanner GetBanner()
    {
        return capi.World.Player?.CurrentBlockSelection == null
            ? null
            : capi.World.BlockAccessor.GetBlockEntity<BlockEntityBanner>(capi.World.Player?.CurrentBlockSelection?.Position);
    }

    public override bool ShouldReceiveRenderEvents() => GetBanner() != null;
    public override bool ShouldReceiveKeyboardEvents() => false;
    public override bool ShouldReceiveMouseEvents() => false;

    private void OnBlockChanged(BlockPos pos, Block oldBlock)
    {
        if (capi.World.Player?.CurrentBlockSelection != null && pos.Equals(capi.World.Player.CurrentBlockSelection.Position))
        {
            ComposeBannerPreviewHud();
        }
        else
        {
            ClearComposers();
        }
    }

    private void Every500ms(float dt) => ComposeBannerPreviewHud();

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();
        Hotkeys.ShowBannerPreviewHud = true;
    }

    public override void OnGuiClosed()
    {
        base.OnGuiClosed();
        Hotkeys.ShowBannerPreviewHud = false;
    }
}
