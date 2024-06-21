using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Flags;

public interface IRotatableBanner
{
    float RotateX { get; set; }
    float RotateY { get; set; }
    float RotateZ { get; set; }

    void RotateWhenPlaced(IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack, BlockEntityBanner blockEntity);
    MeshData RotatedMesh(MeshData mesh);
    Cuboidf RotatedCuboid(Cuboidf cuboid);
    bool TryRotate(EntityAgent byEntity, BlockSelection blockSel, int dir);
    void RotateByAxis(int dir, EnumAxis axis, float rotInterval);
    // void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis);
}