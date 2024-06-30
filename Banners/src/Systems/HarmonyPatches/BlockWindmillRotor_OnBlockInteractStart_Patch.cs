using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;

namespace Flags;

/// <summary>
/// Patch because BlockWindmillRotor doesn't call base.OnBlockInteractStart properly
/// </summary>
public static class BlockWindmillRotor_OnBlockInteractStart_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(BlockWindmillRotor), nameof(BlockWindmillRotor.OnBlockInteractStart), new[] { typeof(IWorldAccessor), typeof(IPlayer), typeof(BlockSelection) });
    }

    public static MethodInfo GetPrefix() => typeof(BlockWindmillRotor_OnBlockInteractStart_Patch).GetMethod(nameof(Prefix));

    public static bool Prefix(BlockBed __instance, ref bool __result, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (blockSel != null
            && byPlayer?.Entity?.ActiveHandItemSlot?.Itemstack?.Collectible is BlockBanner
            && __instance.HasBehavior<BlockBehaviorWindmillWithBanners>()
            && !blockSel.IsProtected(world, byPlayer, EnumBlockAccessFlags.BuildOrBreak)
            && world.BlockAccessor.TryGetBEBehavior(blockSel, out BEBehaviorWindmillWithBanners bebehavior)
            && bebehavior.TryPut(byPlayer.Entity.ActiveHandItemSlot, blockSel))
        {
            __result = true;
            return false;
        }
        return true;
    }
}