using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace Flags;

public class ConfigSystem : ModSystem
{
    public class ModClientSetting
    {
        public const string ShowConfigDialog = "flags:showBannerConfigDialog";
        public const string ShowExtraInfo = "flags:showBannerExtraInfo";

        public const string ShowPreview = "flags:showBannerPreview";
        public const string PreviewAlignment = "flags:bannerPreviewAlignment";
        public const string PreviewX = "flags:bannerPreviewX";
        public const string PreviewY = "flags:bannerPreviewY";

        public const string ShowOverview = "flags:showBannerOverview";
        public const string OverviewAlignment = "flags:bannerOverviewAlignment";
        public const string OverviewX = "flags:bannerOverviewX";
        public const string OverviewY = "flags:bannerOverviewY";
    }

    public static bool ShowBannerConfigDialog
    {
        get => ClientSettings.Inst.GetBoolSetting(ModClientSetting.ShowConfigDialog);
        set => ClientSettings.Inst.Bool[ModClientSetting.ShowConfigDialog] = value;
    }

    public class BannerPreviewConfig
    {
        public static bool Enabled
        {
            get => ClientSettings.Inst.GetBoolSetting(ModClientSetting.ShowPreview);
            set => ClientSettings.Inst.Bool[ModClientSetting.ShowPreview] = value;
        }

        public static EnumDialogArea Alignment
        {
            get => (EnumDialogArea)ClientSettings.Inst.GetIntSetting(ModClientSetting.PreviewAlignment);
            set => ClientSettings.Inst.Int[ModClientSetting.PreviewAlignment] = (int)value;
        }

        public static double X
        {
            get => ClientSettings.Inst.GetIntSetting(ModClientSetting.PreviewX);
            set => ClientSettings.Inst.Int[ModClientSetting.PreviewX] = (int)value;
        }

        public static double Y
        {
            get => ClientSettings.Inst.GetIntSetting(ModClientSetting.PreviewY);
            set => ClientSettings.Inst.Int[ModClientSetting.PreviewY] = (int)value;
        }
    }

    public class BannerOverviewConfig
    {
        public static bool Enabled
        {
            get => ClientSettings.Inst.GetBoolSetting(ModClientSetting.ShowOverview);
            set => ClientSettings.Inst.Bool[ModClientSetting.ShowOverview] = value;
        }

        public static EnumDialogArea Alignment
        {
            get => (EnumDialogArea)ClientSettings.Inst.GetIntSetting(ModClientSetting.OverviewAlignment);
            set => ClientSettings.Inst.Int[ModClientSetting.OverviewAlignment] = (int)value;
        }

        public static double X
        {
            get => ClientSettings.Inst.GetIntSetting(ModClientSetting.OverviewX);
            set => ClientSettings.Inst.Int[ModClientSetting.OverviewX] = (int)value;
        }

        public static double Y
        {
            get => ClientSettings.Inst.GetIntSetting(ModClientSetting.OverviewY);
            set => ClientSettings.Inst.Int[ModClientSetting.OverviewY] = (int)value;
        }
    }

    public class BannerExtraInfoConfig
    {
        public static bool Enabled
        {
            get => ClientSettings.Inst.GetBoolSetting(ModClientSetting.ShowExtraInfo);
            set => ClientSettings.Inst.Bool[ModClientSetting.ShowExtraInfo] = value;
        }
    }

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Input.RegisterHotKey(guiBannerConfigDialog, guiBannerConfigDialog.Localize(), GlKeys.B, HotkeyType.GUIOrOtherControls, shiftPressed: true, ctrlPressed: true);
        api.Input.SetHotKeyHandler(guiBannerConfigDialog, (_) => { ShowBannerConfigDialog = !ShowBannerConfigDialog; return true; });

        if (!api.Settings.Bool.Exists(ModClientSetting.ShowExtraInfo)) BannerExtraInfoConfig.Enabled = true;
        if (!api.Settings.Bool.Exists(ModClientSetting.ShowPreview)) BannerPreviewConfig.Enabled = true;
        if (!api.Settings.Bool.Exists(ModClientSetting.ShowOverview)) BannerOverviewConfig.Enabled = true;

        if (!api.Settings.Int.Exists(ModClientSetting.PreviewAlignment)) BannerPreviewConfig.Alignment = EnumDialogArea.LeftMiddle;
        if (!api.Settings.Int.Exists(ModClientSetting.PreviewX)) BannerPreviewConfig.X = GuiStyle.DialogToScreenPadding;
        if (!api.Settings.Int.Exists(ModClientSetting.PreviewY)) BannerPreviewConfig.Y = GuiStyle.DialogToScreenPadding;

        if (!api.Settings.Int.Exists(ModClientSetting.OverviewAlignment)) BannerOverviewConfig.Alignment = EnumDialogArea.LeftTop;
        if (!api.Settings.Int.Exists(ModClientSetting.OverviewX)) BannerOverviewConfig.X = GuiStyle.DialogToScreenPadding;
        if (!api.Settings.Int.Exists(ModClientSetting.OverviewY)) BannerOverviewConfig.Y = GuiStyle.DialogToScreenPadding;
    }
}