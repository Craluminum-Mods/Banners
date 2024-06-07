using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using System.Linq;

namespace Flags;

public class BEBehaviorBannerContainable : BlockEntityBehavior, IBlockEntityContainer
{
    private InventoryGeneric inv;
    private List<MeshData> meshes = new();

    public List<int> ExcludeFaces { get; protected set; } = new(BlockFacing.NumberOfFaces);
    public string ShapeKey { get; protected set; }

    public RotationsByFace RotationsByFace { get; protected set; } = new();

    public Vec3f GetRotation(BlockFacing facing) => RotationsByFace[facing];

    public IInventory Inventory => inv;
    public string InventoryClassName => bannerContainableInvClassName;

    public BEBehaviorBannerContainable(BlockEntity blockentity) : base(blockentity)
    {
        inv = new InventoryGeneric(BlockFacing.NumberOfFaces, "flags-bannercontainable-0", Api, (id, inv) => new ItemSlotBanner(inv));
    }

    public override void Initialize(ICoreAPI api, JsonObject properties)
    {
        base.Initialize(api, properties);
        inv.LateInitialize($"{InventoryClassName}-{Pos.X}/{Pos.Y}/{Pos.Z}", api);
        inv.Pos = Pos;
        inv.ResolveBlocksOrItems();
        ExcludeFaces = properties[attributeExcludeFaces].AsObject<List<int>>(new(BlockFacing.NumberOfFaces));
        ShapeKey = properties[attributeShapeKey].AsString();
        RotationsByFace = properties[attributeRotationsByFace].AsObject<RotationsByFace>();

        if (meshes == null || !meshes.Any())
        {
            Init();
        }
    }

    protected void Init()
    {
        if (Api == null || Api.Side != EnumAppSide.Client)
        {
            return;
        }

        meshes.Clear();

        for (int faceIndex = 0; faceIndex < inv.Count; faceIndex++)
        {
            ItemSlot slot = inv[faceIndex];
            if (inv != null && faceIndex < inv.Count && !slot.Empty && slot.Itemstack.Collectible is BlockBanner blockBanner)
            {
                BlockFacing facing = BlockFacing.ALLFACES[faceIndex];
                meshes.Add(blockBanner.GetOrCreateContainableMesh(Api, slot.Itemstack, ShapeKey, GetRotation(facing)));
            }
        }
    }

    public override void OnBlockBroken(IPlayer byPlayer = null)
    {
        if (Api?.Side.IsServer() == true)
        {
            DropContents();
        }
    }

    public override void OnBlockRemoved()
    {
        if (Api?.Side.IsServer() == true)
        {
            DropContents();
        }

        // calling MarkDirty and Init in case BlockBed calls OnBlockRemoved in THE MOST STUPID WAY POSSIBLE
        Blockentity?.MarkDirty(true);
        Init();
    }

    public void DropContents(Vec3d atPos = null)
    {
        inv.DropAll(Pos.ToVec3d().Add(atPos ?? new Vec3d(0.5, 0.5, 0.5)));
    }

    public override void OnBlockUnloaded()
    {
        for (int i = 0; i < meshes.Count; i++)
        {
            meshes[i].Dispose();
        }
        meshes.Clear();
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        inv.FromTreeAttributes(tree.GetTreeAttribute(attributeInventoryBannerContainable));
        Init();
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        if (inv != null)
        {
            ITreeAttribute invtree = new TreeAttribute();
            inv.ToTreeAttributes(invtree);
            tree[attributeInventoryBannerContainable] = invtree;
        }
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
    {
        foreach (MeshData mesh in meshes)
        {
            mesher.AddMeshData(mesh);
        }
        return false;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        int faceIndex = forPlayer.CurrentBlockSelection.Face.Index;
        if (Api is ICoreClientAPI && !ExcludeFaces.Contains(faceIndex))
        {
            dsc.AppendLine(langCodeBannerContainableContainedBanner.Localize(!inv[faceIndex].Empty ? inv[faceIndex].Itemstack.GetName() : langCodeEmpty.Localize()));
            dsc.AppendLine(".");
        }
    }

    public bool TryPut(ItemSlot slot, BlockSelection blockSel)
    {
        int faceIndex = blockSel.Face.Index;
        if (inv != null && faceIndex < inv.Count && inv[faceIndex].Empty && !ExcludeFaces.Contains(faceIndex))
        {
            int num = slot.TryPutInto(Api.World, inv[faceIndex]);
            Blockentity.MarkDirty(true);
            return num > 0;
        }
        return false;
    }

    public bool TryTake(BlockSelection blockSel)
    {
        int faceIndex = blockSel.Face.Index;
        if (inv != null && faceIndex < inv.Count && !inv[faceIndex].Empty)
        {
            inv.DropSlots(Pos.ToVec3d().Add(new Vec3d(0.5, 0.5, 0.5)), faceIndex);
            Blockentity.MarkDirty(true);
            return true;
        }
        return false;
    }
}