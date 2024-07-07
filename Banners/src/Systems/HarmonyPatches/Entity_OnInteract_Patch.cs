using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Flags;

public static class Entity_OnInteract_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Entity), nameof(Entity.OnInteract), new[] { typeof(EntityAgent), typeof(ItemSlot), typeof(Vec3d), typeof(EnumInteractMode) });
    }

    public static MethodInfo GetPrefix() => typeof(Entity_OnInteract_Patch).GetMethod(nameof(Prefix));

    public static bool Prefix(Entity __instance, EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
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
