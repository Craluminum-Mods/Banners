using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Flags;

public class Hotkeys : ModSystem
{
    private ICoreClientAPI capi;

    public bool DisplayBannerExtraInfo
    {
        get => capi.Settings.Bool[ModClientSetting.BannerExtraInfo];
        set => capi.Settings.Bool[ModClientSetting.BannerExtraInfo] = value;
    }

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;
        api.Input.RegisterHotKey(ModHotkey.BannerExtraInfo, ModHotkey.BannerExtraInfo.Localize(), GlKeys.B, HotkeyType.HelpAndOverlays, shiftPressed: true);
        api.Input.SetHotKeyHandler(ModHotkey.BannerExtraInfo, ToggleExtraInfo);

        if (!capi.Settings.Bool.Exists(ModClientSetting.BannerExtraInfo))
        {
            DisplayBannerExtraInfo = true;
        }
    }

    public bool ToggleExtraInfo(KeyCombination t1)
    {
        DisplayBannerExtraInfo = !DisplayBannerExtraInfo;
        return true;
    }
}