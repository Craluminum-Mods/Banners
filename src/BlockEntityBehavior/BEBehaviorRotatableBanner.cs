using System;
using System.Collections.Generic;
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

    public void RotateWhenPlaced(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
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
        return blockMesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), RotateX, RotateY * DegreesToRadians, RotateZ);
    }

    public Cuboidf RotatedCuboid(Cuboidf cuboid)
    {
        return cuboid.RotatedCopy(RotateX * RadiansToDegrees, RotateY * RadiansToDegrees, RotateZ * RadiansToDegrees, new Vec3d(0.5, 0.5, 0.5)).ClampTo(Vec3f.Zero, Vec3f.One);
    }

    public bool TryRotate(EntityAgent byEntity, BlockSelection blockSel, int dir)
    {
        bool sneak = byEntity.Controls.Sneak;
        bool sprint = byEntity.Controls.Sprint;

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

    public WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, List<ItemStack> wrenchStacks)
    {
        if (selection.IsProtected(world, forPlayer, EnumBlockAccessFlags.BuildOrBreak))
        {
            return Array.Empty<WorldInteraction>();
        }

        return new WorldInteraction[]
        {
            new()
            {
                ActionLangCode = string.Format("{0} ({1})",  langCodeRotate.Localize(), langCodeRotateBy22_5.Localize()),
                MouseButton = EnumMouseButton.Left,
                Itemstacks = wrenchStacks?.ToArray()
            },
            new()
            {
                ActionLangCode = string.Format("{0} ({1})", langCodeRotateByAxis.Localize(), langCodeRotateBy90.Localize()),
                MouseButton = EnumMouseButton.Left,
                HotKeyCodes = new[] { "shift" },
                Itemstacks = wrenchStacks?.ToArray()
            },
            new()
            {
                ActionLangCode = langCodeClearRotationsXZ,
                MouseButton = EnumMouseButton.Left,
                HotKeyCodes = new[] { "ctrl" },
                Itemstacks = wrenchStacks?.ToArray()
            },
        };
    }

    public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis)
    {
        float thetaX = tree.GetFloat(attributeRotateX);
        float thetaY = tree.GetFloat(attributeRotateY);
        float thetaZ = tree.GetFloat(attributeRotateZ);

        float[] array = Mat4f.Create();
        Mat4f.RotateY(array, array, (float)-degreeRotation * DegreesToRadians);
        Mat4f.RotateX(array, array, thetaX);
        Mat4f.RotateY(array, array, thetaY);
        Mat4f.RotateZ(array, array, thetaZ);
        Mat4f.ExtractEulerAngles(array, ref thetaX, ref thetaY, ref thetaZ);

        tree.SetFloat(attributeRotateX, thetaX);
        tree.SetFloat(attributeRotateY, thetaY);
        tree.SetFloat(attributeRotateZ, thetaZ);
        RotateX = thetaX;
        RotateY = thetaY;
        RotateZ = thetaZ;
    }
}