using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace Flags;

public class Hotkeys : ModSystem
{
    private ICoreClientAPI capi;

    public static bool DisplayBannerExtraInfo
    {
        get => ClientSettings.Inst.GetBoolSetting(ModClientSetting.BannerExtraInfo);
        set => ClientSettings.Inst.Bool[ModClientSetting.BannerExtraInfo] = value;
    }

    public static bool ShowBannerPreviewHud
    {
        get => ClientSettings.Inst.GetBoolSetting(ModClientSetting.ShowBannerPreviewHud);
        set => ClientSettings.Inst.Bool[ModClientSetting.ShowBannerPreviewHud] = value;
    }

    public static bool ShowBannerOverviewHud
    {
        get => ClientSettings.Inst.GetBoolSetting(ModClientSetting.ShowBannerOverviewHud);
        set => ClientSettings.Inst.Bool[ModClientSetting.ShowBannerOverviewHud] = value;
    }

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;
        api.Input.RegisterHotKey(ModHotkey.BannerExtraInfo, ModHotkey.BannerExtraInfo.Localize(), GlKeys.B, HotkeyType.HelpAndOverlays, shiftPressed: true);
        api.Input.SetHotKeyHandler(ModHotkey.BannerExtraInfo, (_) => { DisplayBannerExtraInfo = !DisplayBannerExtraInfo; return true; });

        if (!capi.Settings.Bool.Exists(ModClientSetting.BannerExtraInfo))
        {
            DisplayBannerExtraInfo = true;
        }

        api.Input.RegisterHotKey(ModHotkey.BannerPreviewHud, ModHotkey.BannerPreviewHud.Localize(), GlKeys.P, HotkeyType.HelpAndOverlays, shiftPressed: true);
        api.Input.SetHotKeyHandler(ModHotkey.BannerPreviewHud, (_) => { ShowBannerPreviewHud = !ShowBannerPreviewHud; return true; });

        if (!capi.Settings.Bool.Exists(ModClientSetting.ShowBannerPreviewHud))
        {
            ShowBannerPreviewHud = true;
        }

        api.Input.RegisterHotKey(ModHotkey.BannerOverviewHud, ModHotkey.BannerOverviewHud.Localize(), GlKeys.P, HotkeyType.HelpAndOverlays, shiftPressed: true, ctrlPressed: true);
        api.Input.SetHotKeyHandler(ModHotkey.BannerOverviewHud, (_) => { ShowBannerOverviewHud = !ShowBannerOverviewHud; return true; });

        if (!capi.Settings.Bool.Exists(ModClientSetting.ShowBannerOverviewHud))
        {
            ShowBannerOverviewHud = true;
        }
    }
}