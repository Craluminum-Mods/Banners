using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

public static class BannerPatternExtensions
{
    public static MeshData GetOrCreateMesh(this ItemBannerPattern item, ICoreAPI api, PatternProperties properties)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;

        string key = $"{item.Code}-{properties}";
        if (!item.Meshes.TryGetValue(key, out MeshData mesh))
        {
            CompositeShape rcshape = item.Shape;
            if (rcshape == null)
            {
                capi.Tesselator.TesselateItem(item, out mesh);
                capi.Logger.Error("[Flags] No matching shape found for item {0}", item.Code);
                return mesh;
            }
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Shape shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            ITexPositionSource texSource = item.HandleTextures(properties, capi, shape, rcshape.Base.ToString());
            if (shape == null)
            {
                capi.Tesselator.TesselateItem(item, out mesh);
                capi.Logger.Error("[Flags] Item {0} defines shape '{1}', but no matching shape found", item.Code, rcshape.Base);
                return mesh;
            }
            capi.Tesselator.TesselateShape("Banner pattern item", shape, out mesh, texSource);
            item.Meshes[key] = mesh;
        }
        return mesh;
    }

    public static void GetInventoryMesh(this ItemBannerPattern item, ICoreClientAPI capi, ItemStack stack, ItemRenderInfo renderinfo)
    {
        PatternProperties properties = PatternProperties.FromStack(stack);
        string key = $"{item.Code}-{properties}";
        if (!item.InvMeshes.TryGetValue(key, out MultiTextureMeshRef meshref))
        {
            MeshData mesh = item.GetOrCreateMesh(capi, properties);
            meshref = item.InvMeshes[key] = capi.Render.UploadMultiTextureMesh(mesh);
        }
        renderinfo.ModelRef = meshref;
    }

    public static ITexPositionSource HandleTextures(this ItemBannerPattern item, PatternProperties properties, ICoreClientAPI capi, Shape shape, string filenameForLogging = "")
    {
        ShapeTextureSource texSource = new ShapeTextureSource(capi, shape, filenameForLogging);

        foreach ((string textureCode, CompositeTexture texture) in item.CustomTextures)
        {
            CompositeTexture ctex = texture.Clone();
            if (item.TextureCodesForOverlays.Contains(textureCode))
            {
                item.ReplaceTexture(capi, textureCode, ref ctex, properties);
            }
            ctex.Bake(capi.Assets);
            texSource.textures[textureCode] = ctex;
        }
        return texSource;
    }

    public static void ReplaceTexture(this ItemBannerPattern item, ICoreClientAPI capi, string textureCode, ref CompositeTexture ctex, PatternProperties properties)
    {
        if (!item.CustomTextures.TryGetValue(properties.GetTextureCode(textureCode), out CompositeTexture _newTexture) || _newTexture == null)
        {
            capi.Logger.Error("[Flags] Item {0} defines a texture key '{1}', but no matching texture found", item.Code, properties.GetTextureCode(textureCode));
            ctex.Base = AssetLocation.Create("unknown");
            return;
        }

        CompositeTexture newTexture = _newTexture.Clone();
        newTexture.FillPlaceholder(textureCodePattern, properties.Type);
        ctex = newTexture;
    }
}