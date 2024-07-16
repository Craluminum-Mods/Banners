using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

public static class GuiHandbookItemStackPage_PageCodeForStack_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(GuiHandbookItemStackPage), nameof(GuiHandbookItemStackPage.PageCodeForStack), new[] { typeof(ItemStack) });
    }

    public static MethodInfo GetBase() => typeof(GuiHandbookItemStackPage_PageCodeForStack_Patch).GetMethod(nameof(Base));
    public static MethodInfo GetPostfix() => typeof(GuiHandbookItemStackPage_PageCodeForStack_Patch).GetMethod(nameof(Postfix));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string Base(ItemStack stack)
    {
        return default;
    }

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