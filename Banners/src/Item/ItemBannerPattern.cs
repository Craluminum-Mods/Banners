using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Flags;

public class ItemBannerPattern : Item, IContainedMeshSource
{
    public string RolledShape { get; protected set; }

    public Dictionary<string, List<string>> PatternGroupsBy { get; protected set; } = new();

    public Dictionary<string, CompositeTexture> CustomTextures { get; protected set; } = new();
    public List<string> TextureCodesForOverlays { get; protected set; } = new();

    public Dictionary<string, MeshData> Meshes
    {
        get
        {
            if (api == null) return new();
            return ObjectCacheUtil.GetOrCreate(api, cacheKeyItemBannerPatternMeshes, () => new Dictionary<string, MeshData>());
        }
    }

    public Dictionary<string, MultiTextureMeshRef> InvMeshes
    {
        get
        {
            if (api == null) return new();
            return ObjectCacheUtil.GetOrCreate(api, cacheKeyItemBannerPatternMeshesInv, () => new Dictionary<string, MultiTextureMeshRef>());
        }
    }

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        RolledShape = Attributes[attributeRolledShape].AsString(null);
        LoadTypes();
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        base.OnUnloaded(api);
        PatternGroupsBy?.Clear();
        CustomTextures?.Clear();
        TextureCodesForOverlays?.Clear();

        if (Meshes != null)
        {
            foreach (MeshData mesh in Meshes.Values)
            {
                mesh?.Dispose();
            }
        }
        if (InvMeshes != null)
        {
            foreach (MultiTextureMeshRef meshRef in InvMeshes.Values)
            {
                meshRef?.Dispose();
            }
        }
        ObjectCacheUtil.Delete(api, cacheKeyItemBannerPatternMeshesInv);
    }

    public void LoadTypes()
    {
        PatternGroupsBy = Attributes[attributePatternGroupsBy].AsObject<Dictionary<string, List<string>>>();
        CustomTextures = Attributes[attributeTextures].AsObject<Dictionary<string, CompositeTexture>>();
        TextureCodesForOverlays = Attributes[attributeTextureCodesForOverlays].AsObject<List<string>>();
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder sb, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, sb, world, withDebugInfo);
        PatternProperties props = PatternProperties.FromStack(inSlot.Itemstack);
        if (!string.IsNullOrEmpty(props.Type)
            && PatternGroupsBy.TryGetValueOrWildcard(props.Type, out List<string> patternGroupsByType)
            && patternGroupsByType != null)
        {
            sb.AppendLine(langCodePatternGroups.Localize(string.Join(commaSeparator, patternGroupsByType.Select(group => $"{langCodePatternGroup}{group}".Localize()))));
        }
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        MultiTextureMeshRef meshRef = GetInventoryMesh(itemstack, renderinfo);
        renderinfo.ModelRef = meshRef;
    }

    public MeshData GetOrCreateMesh(PatternProperties properties)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;

        string key = $"{Code}-{properties}";
        if (!Meshes.TryGetValue(key, out MeshData mesh))
        {
            CompositeShape rcshape = Shape;
            if (rcshape == null)
            {
                capi.Tesselator.TesselateItem(this, out mesh);
                capi.Logger.Error("[Flags] No matching shape found for item {0}", Code);
                return mesh;
            }
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Shape shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            ITexPositionSource texSource = HandleTextures(properties, shape, rcshape.Base.ToString());
            if (shape == null)
            {
                capi.Tesselator.TesselateItem(this, out mesh);
                capi.Logger.Error("[Flags] Item {0} defines shape '{1}', but no matching shape found", Code, rcshape.Base);
                return mesh;
            }
            capi.Tesselator.TesselateShape("Banner pattern item", shape, out mesh, texSource);
            Meshes[key] = mesh;
        }
        return mesh;
    }

    public MultiTextureMeshRef GetInventoryMesh(ItemStack stack, ItemRenderInfo renderinfo)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;
        PatternProperties properties = PatternProperties.FromStack(stack);
        string key = $"{Code}-{properties}";
        if (!InvMeshes.TryGetValue(key, out MultiTextureMeshRef meshref))
        {
            MeshData mesh = GetOrCreateMesh(properties);
            meshref = InvMeshes[key] = capi.Render.UploadMultiTextureMesh(mesh);
        }
        return meshref;
    }

    public ITexPositionSource HandleTextures(PatternProperties properties, Shape shape, string filenameForLogging = "")
    {
        ShapeTextureSource texSource = new ShapeTextureSource(api as ICoreClientAPI, shape, filenameForLogging);

        foreach ((string textureCode, CompositeTexture texture) in CustomTextures)
        {
            CompositeTexture ctex = texture.Clone();
            if (TextureCodesForOverlays.Contains(textureCode))
            {
                ReplaceTexture(textureCode, ref ctex, properties);
            }
            ctex.Bake(api.Assets);
            texSource.textures[textureCode] = ctex;
        }
        return texSource;
    }

    public void ReplaceTexture(string textureCode, ref CompositeTexture ctex, PatternProperties properties)
    {
        if (!CustomTextures.TryGetValue(properties.GetTextureCode(textureCode), out CompositeTexture _newTexture) || _newTexture == null)
        {
            api.Logger.Error("[Flags] Item {0} defines a texture key '{1}', but no matching texture found", Code, properties.GetTextureCode(textureCode));
            ctex.Base = AssetLocation.Create("unknown");
            return;
        }

        CompositeTexture newTexture = _newTexture.Clone();
        newTexture.FillPlaceholder(textureCodePattern, properties.Type);
        ctex = newTexture;
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