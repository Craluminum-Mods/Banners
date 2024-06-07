using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

/// <summary>
/// Patch because BlockBed doesn't call base.OnBlockInteractStart
/// </summary>
public static class BlockBed_OnBlockInteractStart_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(BlockBed), nameof(BlockBed.OnBlockInteractStart), new[] { typeof(IWorldAccessor), typeof(IPlayer), typeof(BlockSelection) });
    }

    public static bool Prefix(BlockBed __instance, ref bool __result, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (blockSel != null
            && byPlayer?.Entity?.ActiveHandItemSlot?.Itemstack?.Collectible is BlockBanner
            && world.BlockAccessor.TryGetBlockBehavior(blockSel, out BlockBehaviorBannerContainableInteractions behavior)
            && behavior.Enabled
            && !blockSel.IsProtected(world, byPlayer, EnumBlockAccessFlags.BuildOrBreak)
            && world.BlockAccessor.TryGetBEBehavior(blockSel, out BEBehaviorBannerContainable bebehavior)
            && bebehavior.TryPut(byPlayer.Entity.ActiveHandItemSlot, blockSel))
        {
            __result = true;
            return false;
        }
        return true;
    }
}