using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace Flags;
public class WindmillWithBannersMechBlockRenderer : MechBlockRenderer
{
    private CustomMeshDataPartFloat matrixAndLightFloats;

    private MeshRef blockMeshRef;

    public WindmillWithBannersMechBlockRenderer(ICoreClientAPI capi, MechanicalPowerMod mechanicalPowerMod, Block textureSoureBlock, CompositeShape shapeLoc)
        : base(capi, mechanicalPowerMod)
    {
        AssetLocation shapePath = shapeLoc.Base.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
        Shape shape = Shape.TryGet(capi, shapePath);
        Vec3f meshRotationDeg = new Vec3f(shapeLoc.rotateX, shapeLoc.rotateY, shapeLoc.rotateZ);
        capi.Tesselator.TesselateShape(textureSoureBlock, shape, out var modeldata, meshRotationDeg);
        if (shapeLoc.Overlays != null)
        {
            for (int i = 0; i < shapeLoc.Overlays.Length; i++)
            {
                CompositeShape compositeShape = shapeLoc.Overlays[i];
                meshRotationDeg = new Vec3f(compositeShape.rotateX, compositeShape.rotateY, compositeShape.rotateZ);
                Shape shape2 = Shape.TryGet(capi, compositeShape.Base.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
                capi.Tesselator.TesselateShape(textureSoureBlock, shape2, out var modeldata2, meshRotationDeg);
                modeldata.AddMeshData(modeldata2);
            }
        }

        modeldata.CustomFloats = (matrixAndLightFloats = new CustomMeshDataPartFloat(202000)
        {
            Instanced = true,
            InterleaveOffsets = new int[5] { 0, 16, 32, 48, 64 },
            InterleaveSizes = new int[5] { 4, 4, 4, 4, 4 },
            InterleaveStride = 80,
            StaticDraw = false
        });
        modeldata.CustomFloats.SetAllocationSize(202000);
        blockMeshRef = capi.Render.UploadMesh(modeldata);
    }

    public WindmillWithBannersMechBlockRenderer(ICoreClientAPI capi, MechanicalPowerMod mechanicalPowerMod, IMechanicalPowerRenderable device)
        : base(capi, mechanicalPowerMod)
    {
        AssetLocation shapePath = device.Shape.Base.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
        Shape shape = Shape.TryGet(capi, shapePath);
        Vec3f meshRotationDeg = new Vec3f(device.Shape.rotateX, device.Shape.rotateY, device.Shape.rotateZ);
        capi.Tesselator.TesselateShape(device.Block, shape, out MeshData modeldata, meshRotationDeg);
        if (device.Shape.Overlays != null)
        {
            for (int i = 0; i < device.Shape.Overlays.Length; i++)
            {
                CompositeShape compositeShape = device.Shape.Overlays[i];
                meshRotationDeg = new Vec3f(compositeShape.rotateX, compositeShape.rotateY, compositeShape.rotateZ);
                Shape shape2 = Shape.TryGet(capi, compositeShape.Base.Clone().WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
                capi.Tesselator.TesselateShape(device.Block, shape2, out MeshData modeldata2, meshRotationDeg);
                modeldata.AddMeshData(modeldata2);
            }
        }

        if (device.Block.HasBehavior<BlockBehaviorWindmillWithBanners>() && TryGetBEBehavior(capi.World.BlockAccessor, device.Position, out BEBehaviorWindmillWithBanners bebehavior))
        {
            bebehavior.AddMeshDataTo(ref modeldata, meshRotationDeg);
        }

        modeldata.CustomFloats = (matrixAndLightFloats = new CustomMeshDataPartFloat(202000)
        {
            Instanced = true,
            InterleaveOffsets = new int[5] { 0, 16, 32, 48, 64 },
            InterleaveSizes = new int[5] { 4, 4, 4, 4, 4 },
            InterleaveStride = 80,
            StaticDraw = false
        });
        modeldata.CustomFloats.SetAllocationSize(202000);
        blockMeshRef = capi.Render.UploadMesh(modeldata);
    }

    public static bool TryGetBEBehavior<T>(IBlockAccessor blockAccessor, BlockPos pos, out T behavior) where T : BlockEntityBehavior
    {
        behavior = blockAccessor.GetBlockEntity(pos)?.GetBehavior<T>();
        return behavior != null;
    }

    protected override void UpdateLightAndTransformMatrix(int index, Vec3f distToCamera, float rotation, IMechanicalPowerRenderable dev)
    {
        float num = rotation * (float)dev.AxisSign[0];
        float rotY = rotation * (float)dev.AxisSign[1];
        float num2 = rotation * (float)dev.AxisSign[2];
        if (dev is BEBehaviorMPToggle bEBehaviorMPToggle && ((num == 0f) ^ bEBehaviorMPToggle.isRotationReversed()))
        {
            rotY = MathF.PI;
            num2 = 0f - num2;
        }

        UpdateLightAndTransformMatrix(matrixAndLightFloats.Values, index, distToCamera, dev.LightRgba, num, rotY, num2);
    }

    public override void OnRenderFrame(float deltaTime, IShaderProgram prog)
    {
        UpdateCustomFloatBuffer();
        if (quantityBlocks > 0)
        {
            matrixAndLightFloats.Count = quantityBlocks * 20;
            updateMesh.CustomFloats = matrixAndLightFloats;
            capi.Render.UpdateMesh(blockMeshRef, updateMesh);
            capi.Render.RenderMeshInstanced(blockMeshRef, quantityBlocks);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        blockMeshRef?.Dispose();
    }
}