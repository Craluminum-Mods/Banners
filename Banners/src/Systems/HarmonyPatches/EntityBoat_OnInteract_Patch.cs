using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Flags;

public static class EntityBoat_OnInteract_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(EntityBoat), nameof(EntityBoat.OnInteract), new[] { typeof(EntityAgent), typeof(ItemSlot), typeof(Vec3d), typeof(EnumInteractMode) });
    }

    public static MethodInfo GetPrefix() => typeof(EntityBoat_OnInteract_Patch).GetMethod(nameof(Prefix));

    public static bool Prefix(EntityBoat __instance, EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
    {
        if (!__instance.HasBehavior<EntityBehaviorBoatWithBanner>())
        {
            return true;
        }

        EntityBehaviorBoatWithBanner behavior = __instance.GetBehavior<EntityBehaviorBoatWithBanner>();

        EnumHandling handling = EnumHandling.PassThrough;
        behavior.OnInteract(byEntity, itemslot, hitPosition, mode, ref handling);
        if (handling == EnumHandling.PreventDefault)
        {
            return false;
        }
        return true;
    }
}
