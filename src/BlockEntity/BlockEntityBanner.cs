using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Flags;

public class BlockEntityBanner : BlockEntity
{
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
        Mesh?.Dispose();
    }

    protected void Init()
    {
        if (Api == null || Api.Side != EnumAppSide.Client)
        {
            return;
        }

        MeshData baseMesh = BannerBlock.GetOrCreateMesh(Api, BannerProps);
        IRotatableBanner rotatableBanner = Block.GetInterface<IRotatableBanner>(Api.World, Pos);
        Mesh = rotatableBanner?.RotatedMesh(baseMesh) ?? baseMesh;
    }

    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);
        if (byItemStack?.Attributes != null)
        {
            BannerProps.FromTreeAttribute(byItemStack.Attributes,
                defaultType: BannerBlock.DefaultPlacement,
                defaultToolModes: BannerBlock.DefaultToolModes);
            BannerProperties.ClearPlacement(byItemStack.Attributes);
        }
        Init();
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        BannerProps.ToTreeAttribute(tree);
        base.ToTreeAttributes(tree);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor world)
    {
        base.FromTreeAttributes(tree, world);
        BannerProps.FromTreeAttribute(tree,
            defaultType: BannerBlock.DefaultPlacement,
            defaultToolModes: BannerBlock.DefaultToolModes);
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
        sb.AppendLine(langCodePatternGroups.Localize(string.Join(commaSeparator, BannerBlock.PatternGroups.Select(group => $"{langCodePatternGroup}{group}".Localize()))));
        BannerProps.GetDescription(sb, BannerBlock.ShowDebugInfo);
    }

    public Cuboidf[] GetOrCreateSelectionBoxes(bool forceNew = false)
    {
        if (forceNew || CustomSelectionBoxes == null)
        {
            Cuboidf[] _selectionBoxes = BannerBlock.CustomSelectionBoxes.TryGetValueOrWildcard(BannerProps.Placement, out Cuboidf[] boxes)
            ? boxes
            : Block.SelectionBoxes;

            _selectionBoxes ??= Array.Empty<Cuboidf>();
            CustomSelectionBoxes = new Cuboidf[_selectionBoxes.Length];

            for (int i = 0; i < CustomSelectionBoxes.Length; i++)
            {
                IRotatableBanner rotatableBanner = Block.GetInterface<IRotatableBanner>(Api.World, Pos);
                CustomSelectionBoxes[i] = rotatableBanner?.RotatedCuboid(_selectionBoxes[i]) ?? _selectionBoxes[i];
            }
        }
        return CustomSelectionBoxes;
    }

    public Cuboidf[] GetOrCreateCollisionBoxes(bool forceNew = false)
    {
        if (forceNew || CustomCollisionBoxes == null)
        {
            Cuboidf[] _collisionBoxes = BannerBlock.CustomCollisionBoxes.TryGetValueOrWildcard(BannerProps.Placement, out Cuboidf[] boxes)
            ? boxes
            : Block.CollisionBoxes;

            _collisionBoxes ??= Array.Empty<Cuboidf>();
            CustomCollisionBoxes = new Cuboidf[_collisionBoxes.Length];

            for (int i = 0; i < CustomCollisionBoxes.Length; i++)
            {
                IRotatableBanner rotatableBanner = Block.GetInterface<IRotatableBanner>(Api.World, Pos);
                CustomCollisionBoxes[i] = rotatableBanner?.RotatedCuboid(_collisionBoxes[i]) ?? _collisionBoxes[i];
            }
        }
        return CustomCollisionBoxes;
    }
}