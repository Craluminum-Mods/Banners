using HarmonyLib;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

public static class FixBannerInHandbook
{
    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(typeof(GuiHandbookItemStackPage), nameof(GuiHandbookItemStackPage.PageCodeForStack))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Base(ItemStack stack) => default;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GuiHandbookItemStackPage), nameof(GuiHandbookItemStackPage.PageCodeForStack))]
    public static void Postfix(ref string __result, ItemStack stack)
    {
        if (stack?.Collectible is not BlockBanner)
        {
            return;
        }

        ItemStack newStack = stack.Clone();

        newStack.Attributes.GetTreeAttribute(attributeBanner)?.RemoveAttribute(attributeName);
        newStack.Attributes.GetTreeAttribute(attributeBanner)?.RemoveAttribute(attributeCutouts);
        newStack.Attributes.RemoveAttribute(attributeBannerModes);
        newStack.Attributes.RemoveAttribute(attributeRotX);
        newStack.Attributes.RemoveAttribute(attributeRotY);
        newStack.Attributes.RemoveAttribute(attributeRotZ);

        BannerProperties fromProps = BannerProperties.FromStack(newStack);
        if (fromProps.Patterns.Count <= 1)
        {
            __result = Base(newStack);
            return;
        }

        while (fromProps.Patterns.TryRemoveLast()) { }
        newStack.Attributes.GetTreeAttribute(attributeBanner)?.RemoveAttribute(attributeLayers);
        fromProps.Patterns.ToTreeAttribute(newStack.Attributes.GetTreeAttribute(attributeBanner));

        __result = Base(newStack);
    }
}