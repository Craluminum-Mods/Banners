using HarmonyLib;
using System.Reflection;
using Vintagestory.API.MathTools;

namespace Flags;

/// <summary>
/// Temporary solution for banner cutouts
/// </summary>
public static class ColorBlend_ColorBurn_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(ColorBlend), nameof(ColorBlend.ColorBurn), new[] { typeof(int), typeof(int) });
    }

    public static MethodInfo GetPrefix() => typeof(ColorBlend_ColorBurn_Patch).GetMethod(nameof(Prefix));

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