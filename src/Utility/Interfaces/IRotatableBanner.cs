using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Flags;

public interface IRotatableBanner : IRotatable
{
    float RotateX { get; set; }
    float RotateY { get; set; }
    float RotateZ { get; set; }

    void RotateWhenPlaced(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack);
    MeshData RotatedMesh(MeshData mesh);
    Cuboidf RotatedCuboid(Cuboidf cuboid);
    bool TryRotate(EntityAgent byEntity, BlockSelection blockSel, int dir);
    void RotateByAxis(int dir, EnumAxis axis, float rotInterval);
}