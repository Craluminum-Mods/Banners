using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Flags;

public class BEBehaviorRotatableBanner : BlockEntityBehavior, IRotatableBanner
{
    public float RotateX { get; set; }
    public float RotateY { get; set; }
    public float RotateZ { get; set; }

    public BEBehaviorRotatableBanner(BlockEntity blockentity) : base(blockentity) { }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        tree.SetFloat(attributeRotateX, RotateX);
        tree.SetFloat(attributeRotateY, RotateY);
        tree.SetFloat(attributeRotateZ, RotateZ);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
    {
        RotateX = tree.GetFloat(attributeRotateX);
        RotateY = tree.GetFloat(attributeRotateY);
        RotateZ = tree.GetFloat(attributeRotateZ);
    }

    public void RotateWhenPlaced(IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack, BlockEntityBanner blockEntity)
    {
        float rotInterval = byPlayer.Entity.Controls.Sneak ? Radians22_5 : Radians90;

        BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
        float roundRad = (float)(int)Math.Round((float)Math.Atan2(byPlayer.Entity.Pos.X - ((double)targetPos.X + blockSel.HitPosition.X), (double)(float)byPlayer.Entity.Pos.Z - ((double)targetPos.Z + blockSel.HitPosition.Z)) / rotInterval) * rotInterval;
        RotateX = 0;
        RotateY = roundRad;
        RotateZ = 0;
    }

    public MeshData RotatedMesh(MeshData blockMesh)
    {
        return blockMesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), RotateX, RotateY, RotateZ);
    }

    public Cuboidf RotatedCuboid(Cuboidf cuboid)
    {
        return cuboid.RotatedCopy(RotateX * RadiansToDegrees, RotateY * RadiansToDegrees, RotateZ * RadiansToDegrees, new Vec3d(0.5, 0.5, 0.5)).ClampTo(Vec3f.Zero, Vec3f.One);
    }

    public bool TryRotate(EntityAgent byEntity, BlockSelection blockSel, int dir)
    {
        bool sneak = byEntity.Controls.Sneak;
        bool sprint = byEntity.Controls.Sprint;

        if (Blockentity is not BlockEntityBanner blockEntityBanner || !blockEntityBanner.IsEditModeEnabled())
        {
            return false;
        }

        if (sneak && !sprint)
        {
            RotateByAxis(dir, blockSel.Face.Axis, Radians90);
            return true;
        }
        else if (!sneak && !sprint)
        {
            RotateByAxis(dir, EnumAxis.Y, Radians22_5);
            return true;
        }
        else if (!sneak && sprint)
        {
            RotateX = 0;
            RotateZ = 0;
            return true;
        }

        return false;
    }

    public void RotateByAxis(int dir, EnumAxis axis, float rotInterval)
    {
        switch (axis)
        {
            case EnumAxis.X:
                RotateX += rotInterval * (float)dir;
                break;
            case EnumAxis.Y:
                RotateY += rotInterval * (float)dir;
                break;
            case EnumAxis.Z:
                RotateZ += rotInterval * (float)dir;
                break;
        }
    }
}