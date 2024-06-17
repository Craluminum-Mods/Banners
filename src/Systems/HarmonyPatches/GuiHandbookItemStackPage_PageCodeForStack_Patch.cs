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

        BannerProperties.GetBannerTree(newStack.Attributes).RemoveAttribute(attributeName);
        BannerProperties.GetBannerTree(newStack.Attributes).RemoveAttribute(attributeBannerModes);
        BannerProperties.GetBannerTree(newStack.Attributes).RemoveAttribute(attributeCutouts);
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
        BannerProperties.GetBannerTree(newStack.Attributes).RemoveAttribute(attributeLayers);
        fromProps.Patterns.ToTreeAttribute(BannerProperties.GetBannerTree(newStack.Attributes));

        __result = Base(newStack);
    }
}