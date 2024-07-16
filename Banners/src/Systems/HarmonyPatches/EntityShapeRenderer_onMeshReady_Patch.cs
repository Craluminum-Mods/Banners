using HarmonyLib;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace Flags;

public static class EntityShapeRenderer_onMeshReady_Patch
{
    public static MethodBase TargetMethod()
    {
        return typeof(EntityShapeRenderer).GetMethod("onMeshReady", AccessTools.all, new[] { typeof(MeshData) });
    }

    public static MethodInfo GetPrefix() => typeof(EntityShapeRenderer_onMeshReady_Patch).GetMethod(nameof(Prefix));

    public static bool Prefix(EntityShapeRenderer __instance, MeshData meshData, ref MultiTextureMeshRef ___meshRefOpaque)
    {
        if (!__instance.entity.HasBehavior<EntityBehaviorBoatWithBanner>())
        {
            return true;
        }

        EntityBehaviorBoatWithBanner behavior = __instance.entity.GetBehavior<EntityBehaviorBoatWithBanner>();

        if (!behavior.Inventory.Any())
        {
            return true;
        }

        if (___meshRefOpaque != null)
        {
            ___meshRefOpaque.Dispose();
            ___meshRefOpaque = null;
        }

        if (!__instance.capi.IsShuttingDown && meshData.VerticesCount > 0)
        {
            try
            {
                behavior.AddMeshDataTo(ref meshData);
                ___meshRefOpaque = __instance.capi.Render.UploadMultiTextureMesh(meshData);
            }
            catch (System.Exception) { }
        }

        return false;
    }
}
