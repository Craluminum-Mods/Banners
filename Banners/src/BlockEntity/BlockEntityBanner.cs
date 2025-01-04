using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Flags;

public class BlockEntityBanner : BlockEntity, IRotatable
{
    public string EditMode { get; protected set; } = "on";
    public BannerProperties BannerProps { get; protected set; } = new BannerProperties();
    public BlockBanner BannerBlock => Block as BlockBanner;

    public Cuboidf[] CustomSelectionBoxes { get; protected set; }
    public Cuboidf[] CustomCollisionBoxes { get; protected set; }

    public MeshData Mesh { get; protected set; }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (Mesh == null)
        {
            Init();
        }
    }

    public void ReplaceProperties(BannerProperties fromProps, string editMode = "on")
    {
        EditMode = editMode;
        BannerProps = fromProps;
        MarkDirty(redrawOnClient: true);
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        CustomSelectionBoxes = null;
        CustomCollisionBoxes = null;
        Mesh?.Dispose();
    }

    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        CustomSelectionBoxes = null;
        CustomCollisionBoxes = null;
    }

    protected void Init()
    {
        if (Api == null || Api.Side != EnumAppSide.Client || BannerBlock == null)
        {
            return;
        }

        GetOrCreateCollisionBoxes(true);
        GetOrCreateSelectionBoxes(true);
        MeshData baseMesh = BannerBlock.GetOrCreateMesh(Api, BannerProps);
        IRotatableBanner rotatableBanner = Block.GetInterface<IRotatableBanner>(Api.World, Pos);
        Mesh = rotatableBanner?.RotatedMesh(baseMesh) ?? baseMesh;
    }

    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);
        if (byItemStack != null)
        {
            BannerProps.FromTreeAttribute(byItemStack.Attributes, defaultType: BannerBlock.DefaultPlacement);
            BannerProperties.ClearPlacement(byItemStack.Attributes);
            EditMode = byItemStack.Attributes.GetAsString("editmode", "on");
        }
        Init();
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        BannerProps.ToTreeAttribute(tree);
        tree.SetString("editmode", EditMode);
        base.ToTreeAttributes(tree);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
    {
        base.FromTreeAttributes(tree, world);
        BannerProps = new BannerProperties();
        BannerProps.FromTreeAttribute(tree, defaultType: BannerBlock.DefaultPlacement);
        EditMode = tree.GetAsString("editmode", "on");
        Init();
    }

    public override void MarkDirty(bool redrawOnClient = false, IPlayer skipPlayer = null)
    {
        base.MarkDirty(redrawOnClient, skipPlayer);
        if (redrawOnClient)
        {
            Init();
        }
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        mesher.AddMeshData(Mesh);
        base.OnTesselation(mesher, tesselator);
        return true;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        base.GetBlockInfo(forPlayer, sb);
        BannerProps.GetDescription(BannerBlock, forPlayer, sb, BannerBlock.ShowDebugInfo);
        GetModesDescription(sb);
    }

    public void GetModesDescription(StringBuilder sb)
    {
        if (Api is ICoreClientAPI && ConfigSystem.BannerExtraInfoConfig.Enabled)
        {
            sb.AppendLine($"{langCodeToolMode}{"editmode"}".Localize($"{langCodeToolModeValue}{EditMode}".Localize()));
        }
    }

    public Cuboidf[] GetOrCreateSelectionBoxes(bool forceNew = false)
    {
        if (forceNew || CustomSelectionBoxes == null)
        {
            Cuboidf[] _selectionBoxes = BannerBlock?.CustomSelectionBoxes?.TryGetValueOrWildcard(BannerProps?.Placement ?? "", out Cuboidf[] boxes) == true
            ? boxes
            : Block?.SelectionBoxes;

            _selectionBoxes ??= Array.Empty<Cuboidf>();
            CustomSelectionBoxes = new Cuboidf[_selectionBoxes.Length];

            for (int i = 0; i < CustomSelectionBoxes.Length; i++)
            {
                IRotatableBanner rotatableBanner = Block?.GetInterface<IRotatableBanner>(Api.World, Pos);
                CustomSelectionBoxes[i] = rotatableBanner?.RotatedCuboid(_selectionBoxes[i]) ?? _selectionBoxes[i];
            }
        }
        return CustomSelectionBoxes;
    }

    public Cuboidf[] GetOrCreateCollisionBoxes(bool forceNew = false)
    {
        if (forceNew || CustomCollisionBoxes == null)
        {
            Cuboidf[] _collisionBoxes = BannerBlock?.CustomCollisionBoxes?.TryGetValueOrWildcard(BannerProps?.Placement ?? "", out Cuboidf[] boxes) == true
            ? boxes
            : Block?.CollisionBoxes;

            _collisionBoxes ??= Array.Empty<Cuboidf>();
            CustomCollisionBoxes = new Cuboidf[_collisionBoxes.Length];

            for (int i = 0; i < CustomCollisionBoxes.Length; i++)
            {
                IRotatableBanner rotatableBanner = Block?.GetInterface<IRotatableBanner>(Api.World, Pos);
                CustomCollisionBoxes[i] = rotatableBanner?.RotatedCuboid(_collisionBoxes[i]) ?? _collisionBoxes[i];
            }
        }
        return CustomCollisionBoxes;
    }

    public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis)
    {
        BEBehaviorRotatableBanner rotatableBanner = GetBehavior<BEBehaviorRotatableBanner>();
        int dir = degreeRotation switch
        {
            90 => 1,
            270 => -1,
            _ => 1
        };
        rotatableBanner?.RotateByAxis(dir, EnumAxis.Y, Radians90);
        MarkDirty(redrawOnClient: true);
    }

    public bool IsEditModeEnabled(bool printError = true)
    {
        if (EditMode == "off" && printError)
        {
            string errorCode = "flags:ingameerror-editmode-off";
            (Api as ICoreClientAPI)?.IngameError(this, errorCode, errorCode.Localize());
        }
        return EditMode == "on";
    }
}