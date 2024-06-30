using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Flags;

public class BEBehaviorWindmillWithBanners : BlockEntityBehavior, IBlockEntityContainer
{
    private InventoryGeneric inv;
    private List<MeshData> meshes = new();

    public IInventory Inventory => inv;
    public string InventoryClassName => windmillBannersInvClassName;

    public int sailCount = 5;

    public BEBehaviorWindmillWithBanners(BlockEntity blockentity) : base(blockentity)
    {
        inv = new InventoryGeneric(sailCount, $"{windmillBannersInvClassName}-0", Api, (id, inv) => new ItemSlotBanner(inv));
    }

    public override void Initialize(ICoreAPI api, JsonObject properties)
    {
        base.Initialize(api, properties);
        inv.LateInitialize($"{InventoryClassName}-{Pos.X}/{Pos.Y}/{Pos.Z}", api);
        inv.Pos = Pos;
        inv.ResolveBlocksOrItems();

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
                meshes.Add(GetOrCreateBannerInWindmillMesh(blockBanner, Api, slot.Itemstack, Blockentity));
            }
        }
    }

    public static MeshData GetOrCreateBannerInWindmillMesh(BlockBanner block, ICoreAPI api, ItemStack stack, BlockEntity blockentity)
    {
        BannerProperties properties = BannerProperties.FromStack(stack);

        // BEBehaviorWindmillRotor rotorBehavior = blockentity.GetBehavior<BEBehaviorWindmillRotor>();
        // if (rotorBehavior != null)
        // {
        // }

        return block.GetOrCreateMesh(api, properties);
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
        inv.FromTreeAttributes(tree.GetTreeAttribute(attributeInventoryWindmillBanners));
        Init();
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        if (inv != null)
        {
            ITreeAttribute invtree = new TreeAttribute();
            inv.ToTreeAttributes(invtree);
            tree[attributeInventoryWindmillBanners] = invtree;
        }
    }

    public void AddMeshDataTo(ref MeshData toMeshData, Vec3f meshRotationDeg)
    {
        foreach (MeshData mesh in meshes)
        {
            toMeshData.AddMeshData(mesh.Clone().Rotate(origin: new Vec3f(0.5f, 0.5f, 0.5f), radX: meshRotationDeg.X, radY: meshRotationDeg.Y, radZ: meshRotationDeg.Z));
        }
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        if (Api is not ICoreClientAPI)
        {
            return;
        }
        foreach (ItemSlot slot in inv)
        {
            dsc.AppendLine(!slot.Empty ? langCodeBannerContainableContainedBanner.Localize(slot.Itemstack.GetName()) : langCodeEmpty.Localize());
        }
        dsc.AppendLine(".");
    }

    public bool TryPut(ItemSlot slot, BlockSelection blockSel)
    {
        if (inv != null && inv.Any(slot => slot.Empty))
        {
            int num = slot.TryPutInto(Api.World, inv.First(slot => slot.Empty));
            Init();
            Blockentity.MarkDirty(redrawOnClient: true);
            return num > 0;
        }
        return false;
    }
}