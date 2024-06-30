using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent.Mechanics;

namespace Flags;

public static class MechNetworkRenderer_AddDevice_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(MechNetworkRenderer), nameof(MechNetworkRenderer.AddDevice), new[] { typeof(IMechanicalPowerRenderable) });
    }

    public static MethodInfo GetPrefix() => typeof(MechNetworkRenderer_AddDevice_Patch).GetMethod(nameof(Prefix));

    public static bool Prefix(
        MechNetworkRenderer __instance,
        IMechanicalPowerRenderable device,
        ICoreClientAPI ___capi,
        MechanicalPowerMod ___mechanicalPowerMod,
        ref List<MechBlockRenderer> ___MechBlockRenderer,
        ref Dictionary<int, int> ___MechBlockRendererByShape)
    {
        if (device.Shape == null)
        {
            return true;
        }

        int index = -1;
        string rendererCode = "generic";
        JsonObject attributes = device.Block.Attributes;
        if (attributes != null && attributes["mechanicalPower"]?["renderer"].Exists == true)
        {
            rendererCode = device.Block.Attributes?["mechanicalPower"]?["renderer"].AsString("generic");
        }
        if (rendererCode == "windmillwithbanners" && device.Block.HasBehavior<BlockBehaviorWindmillWithBanners>() && ___capi.World.BlockAccessor.TryGetBEBehavior(device.Position, out BEBehaviorWindmillWithBanners bebehavior))
        {
            int hashCode = device.Block.Code.ToString().GetHashCode() + device.Shape.GetHashCode() + bebehavior.properties.ToString().GetHashCode() + rendererCode.GetHashCode();
            if (!___MechBlockRendererByShape.TryGetValue(hashCode, out index))
            {
                object obj = Activator.CreateInstance(MechNetworkRenderer.RendererByCode[rendererCode], ___capi, ___mechanicalPowerMod, device);
                ___MechBlockRenderer.Add((MechBlockRenderer)obj);
                index = ___MechBlockRendererByShape[hashCode] = ___MechBlockRenderer.Count - 1;
            }
            ___MechBlockRenderer[index].AddDevice(device);
        }
        else
        {
            int hashCode = device.Block.Code.ToString().GetHashCode() + device.Shape.GetHashCode() + rendererCode.GetHashCode();
            if (!___MechBlockRendererByShape.TryGetValue(hashCode, out index))
            {
                object obj = Activator.CreateInstance(MechNetworkRenderer.RendererByCode[rendererCode], ___capi, ___mechanicalPowerMod, device.Block, device.Shape);
                ___MechBlockRenderer.Add((MechBlockRenderer)obj);
                index = ___MechBlockRendererByShape[hashCode] = ___MechBlockRenderer.Count - 1;
            }
            ___MechBlockRenderer[index].AddDevice(device);
        }

        return false;
    }
}