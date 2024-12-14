using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

[HarmonyPatch(typeof(BlockBed), nameof(BlockBed.OnBlockInteractStart))]
public static class ApplyBannerToBedPatch
{
    [HarmonyPrefix]
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