using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Flags;

public class ItemRollableFixed : Item, IContainedMeshSource
{
    public string RolledShape { get; protected set; }

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        RolledShape = Attributes[attributeRolledShape].AsString(null);
    }

    public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos)
    {
        if (!Attributes.KeyExists(attributeRolledShape))
        {
            return null;
        }
        ICoreClientAPI obj = api as ICoreClientAPI;
        AssetLocation loc = AssetLocation.Create(Attributes[attributeRolledShape].AsString(null), Code.Domain).WithPathPrefixOnce(prefixShapes).WithPathAppendixOnce(appendixJson);
        Shape shape = obj.Assets.TryGet(loc).ToObject<Shape>(null);
        ContainedTextureSource cnts = new ContainedTextureSource(obj, targetAtlas, itemstack.Item.Textures.ToDictionary(x => x.Key, x => x.Value.Base), $"For displayed item {Code}");
        obj.Tesselator.TesselateShape(new TesselationMetaData
        {
            TexSource = cnts
        }, shape, out MeshData meshdata);
        return meshdata;
    }

    public string GetMeshCacheKey(ItemStack itemstack) => $"{Code}-{RolledShape}";
}
