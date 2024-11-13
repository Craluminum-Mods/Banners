using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Flags;

public static class CollectibleObjectPatches
{
    public static void PatchLiquidDescription(this CollectibleObject obj)
    {
        bool isLiquid = BannerLiquid.HasAttribute(obj);
        bool hasBehavior = obj.HasBehavior<CollectibleBehaviorBannerLiquidDescription>();
        if (!isLiquid && obj is not BlockLiquidContainerTopOpened)
        {
            return;
        }
        if (isLiquid)
        {
            foreach (CreativeTabAndStackList item in obj.CreativeInventoryStacks)
            {
                item.Tabs = item?.Tabs?.Append(modCreativeTab);
            }
        }
        if (!hasBehavior)
        {
            obj.CollectibleBehaviors = obj.CollectibleBehaviors.Append(new CollectibleBehaviorBannerLiquidDescription(obj));
        }
    }

    public static void PatchCutoutTool(this CollectibleObject obj, ref bool added)
    {
        if (obj is not ItemShears or ItemScythe || obj.HasBehavior<CollectibleBehaviorCutoutTool>())
        {
            return;
        }

        obj.CollectibleBehaviors = obj.CollectibleBehaviors.Append(new CollectibleBehaviorCutoutTool(obj));

        if (!added)
        {
            added = true;
            obj.CreativeInventoryTabs = obj.CreativeInventoryTabs.Append(modCreativeTab);
        }
    }

    public static void PatchWrenchTool(this CollectibleObject obj, ref bool added)
    {
        if (obj is ItemWrench && !added)
        {
            added = true;
            obj.CreativeInventoryTabs = obj.CreativeInventoryTabs.Append(modCreativeTab);
        }
    }

    public static void PatchRenameTool(this CollectibleObject obj)
    {
        if (obj is ItemBook && !obj.HasBehavior<CollectibleBehaviorRenameTool>())
        {
            obj.CollectibleBehaviors = obj.CollectibleBehaviors.Append(new CollectibleBehaviorRenameTool(obj));
        }
    }
}