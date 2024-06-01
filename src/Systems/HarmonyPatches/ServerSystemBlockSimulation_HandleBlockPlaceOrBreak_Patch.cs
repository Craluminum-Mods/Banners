using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Common;
using Vintagestory.Server;

namespace Flags;

/// <summary>
/// Patch to handle single hit of the block
/// </summary>
public static class ServerSystemBlockSimulation_HandleBlockPlaceOrBreak_Patch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(ServerSystemBlockSimulation), "HandleBlockPlaceOrBreak", new[] { typeof(Packet_Client), typeof(ConnectedClient) });
    }

    public static void Prefix(ServerSystemBlockSimulation __instance, Packet_Client packet, ConnectedClient client)
    {
        Packet_ClientBlockPlaceOrBreak p = packet.BlockPlaceOrBreak;
        BlockSelection blockSel = new BlockSelection
        {
            DidOffset = p.DidOffset > 0,
            Face = BlockFacing.ALLFACES[p.OnBlockFace],
            Position = new BlockPos(p.X, p.Y, p.Z),
            HitPosition = new Vec3d(CollectibleNet.DeserializeDouble(p.HitX), CollectibleNet.DeserializeDouble(p.HitY), CollectibleNet.DeserializeDouble(p.HitZ)),
            SelectionBoxIndex = p.SelectionBoxIndex
        };

        if (blockSel != null
            && !blockSel.IsProtected(client.Player.Entity.World, client.Player, EnumBlockAccessFlags.BuildOrBreak)
            && client.Player.Entity.World.BlockAccessor.TryGetBEBehavior(blockSel, out BEBehaviorBannerContainable bebehavior))
        {
            bebehavior.TryTake(blockSel);
        }
    }
}