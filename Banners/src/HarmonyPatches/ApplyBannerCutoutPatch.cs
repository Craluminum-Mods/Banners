using HarmonyLib;
using Vintagestory.API.MathTools;

namespace Flags;

public static class ApplyBannerCutoutPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ColorBlend), nameof(ColorBlend.ColorBurn))]
    public static bool Prefix(ref int __result, int rgb1, int rgb2)
    {
        __result = OverlayCutout(rgb1, rgb2);
        return false;
    }

    public static int OverlayCutout(int rgb1, int rgb2)
    {
        VSColor lhs = new(rgb1);
        VSColor rhs = new(rgb2);
        if (rhs.A != 0)
        {
            lhs.A = 0;
        }
        return lhs.AsInt;
    }
}