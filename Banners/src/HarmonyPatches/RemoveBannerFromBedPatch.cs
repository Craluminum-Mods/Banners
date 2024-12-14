using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Common;
using Vintagestory.Server;

namespace Flags;

[HarmonyPatch(typeof(ServerSystemBlockSimulation), "HandleBlockPlaceOrBreak")]
public static class RemoveBannerFromBedPatch
{
    [HarmonyPrefix]
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