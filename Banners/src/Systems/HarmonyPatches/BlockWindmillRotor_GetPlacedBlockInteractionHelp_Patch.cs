using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent.Mechanics;

namespace Flags;

public static class BlockWindmillRotor_GetPlacedBlockInteractionHelp_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(BlockWindmillRotor), nameof(BlockWindmillRotor.GetPlacedBlockInteractionHelp), new[] { typeof(IWorldAccessor), typeof(BlockSelection), typeof(IPlayer) });
    }

    public static MethodInfo GetPostfix() => typeof(BlockWindmillRotor_GetPlacedBlockInteractionHelp_Patch).GetMethod(nameof(Postfix));

    public static void Postfix(BlockWindmillRotor __instance, ref WorldInteraction[] __result, IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
    {
        BlockBehaviorWindmillWithBanners behavior = __instance.GetBehavior<BlockBehaviorWindmillWithBanners>();
        if (behavior != null)
        {
            EnumHandling handling = EnumHandling.PassThrough;
            __result = __result.Append(behavior.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling));
        }
    }
}