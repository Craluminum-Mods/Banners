using Flags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Banners;

public class ItemBanner : ItemWearable, IAttachableToEntity, IWearableShapeSupplier, IBannerExtraThings
{
    Shape nowTesselatingShape;

    public string DefaultPlacement => (OwnBlock as IBannerExtraThings)?.DefaultPlacement;

    public BlockBanner OwnBlock { get => GetOwnBlock(); }

    public BlockBanner GetOwnBlock()
    {
        JsonItemStack ownBlock = Attributes?["ownBlock"]?.AsObject<JsonItemStack>();
        if (!ownBlock.Resolve(api.World, ""))
        {
            return null;
        }
        return ownBlock?.ResolvedItemstack?.Block as BlockBanner;
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder sb, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, sb, world, withDebugInfo);
        BannerProperties.FromStack(inSlot.Itemstack).GetDescription(OwnBlock, (world as IClientWorldAccessor)?.Player, sb, OwnBlock.ShowDebugInfo);
        GetModesDescription(inSlot, sb, world);
    }

    public void GetModesDescription(ItemSlot slot, StringBuilder sb, IWorldAccessor world)
    {
        if (world.Api is ICoreClientAPI && ConfigSystem.BannerExtraInfoConfig.Enabled)
        {
            string editMode = slot.Itemstack.Attributes.GetAsString("editmode", "on");
            sb.AppendLine($"{langCodeToolMode}{"editmode"}".Localize($"{langCodeToolModeValue}{editMode}".Localize()));
        }
    }

    public override bool Equals(ItemStack thisStack, ItemStack otherStack, params string[] ignoreAttributeSubTrees)
    {
        ignoreAttributeSubTrees ??= Array.Empty<string>();
        ignoreAttributeSubTrees = ignoreAttributeSubTrees.Append("editmode");
        return base.Equals(thisStack, otherStack, ignoreAttributeSubTrees);
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        Dictionary<string, MultiTextureMeshRef> meshrefs = ObjectCacheUtil.GetOrCreate(capi, "wearableAttachmentMeshRefs", () => new Dictionary<string, MultiTextureMeshRef>());
        string key = GetMeshCacheKey(itemstack);

        if (!meshrefs.TryGetValue(key, out renderinfo.ModelRef))
        {
            BannerProperties properties = BannerProperties.FromStack(itemstack);
            Shape shape = GetShapeFromAttributes(itemstack);
            UniversalShapeTextureSource texSource = HandleTextures(properties, api, shape, "");
            MeshData mesh = genMesh(capi, itemstack, texSource);
            renderinfo.ModelRef = meshrefs[key] = mesh == null ? renderinfo.ModelRef : capi.Render.UploadMultiTextureMesh(mesh);
        }

        if (Attributes["visibleDamageEffect"].AsBool())
        {
            renderinfo.DamageEffect = Math.Max(0, 1 - (float)GetRemainingDurability(itemstack) / GetMaxDurability(itemstack) * 1.1f);
        }

        if (target == EnumItemRenderTarget.Gui)
        {
            renderinfo.NormalShaded = false;
        }
    }

    protected new MeshData genMesh(ICoreClientAPI capi, ItemStack itemstack, ITexPositionSource texSource)
    {
        JsonObject attrObj = itemstack.Collectible.Attributes;
        EntityProperties props = capi.World.GetEntityType(new AssetLocation(attrObj?["wearerEntityCode"].ToString() ?? "player"));
        Shape entityShape = props.Client.LoadedShape;
        AssetLocation shapePathForLogging = props.Client.Shape.Base;
        Shape newShape = new Shape()
        {
            Elements = entityShape.CloneElements(),
            Animations = entityShape.CloneAnimations(),
            AnimationsByCrc32 = entityShape.AnimationsByCrc32,
            JointsById = entityShape.JointsById,
            TextureWidth = entityShape.TextureWidth,
            TextureHeight = entityShape.TextureHeight,
            Textures = null,
        };

        Shape armorShape = GetShapeFromAttributes(itemstack);

        if (null == armorShape) return new MeshData();

        newShape.StepParentShape(armorShape, "foo", shapePathForLogging.ToShortString(), capi.Logger, (key, code) => { });

        nowTesselatingShape = newShape;
        capi.Tesselator.TesselateShapeWithJointIds("entity", newShape, out MeshData mesh, texSource, new Vec3f());
        nowTesselatingShape = null;

        return mesh;
    }

    public new string GetMeshCacheKey(ItemStack itemstack)
    {
        BannerProperties props = BannerProperties.FromStack(itemstack);
        string compactKey = props.ToCompactString();
        return $"item-{itemstack.Collectible.Code}-{compactKey}";
    }

    public UniversalShapeTextureSource HandleTextures(BannerProperties properties, ICoreAPI api, Shape shape, string filenameForLogging = "")
    {
        ICoreClientAPI capi = api as ICoreClientAPI;
        UniversalShapeTextureSource texSource = new UniversalShapeTextureSource(capi, capi.ItemTextureAtlas, shape, filenameForLogging);

        foreach ((string textureCode, CompositeTexture texture) in Textures)
        {
            texSource.textures[textureCode] = texture;
        }

        foreach ((string textureCode, CompositeTexture texture) in OwnBlock.CustomTextures)
        {
            CompositeTexture ctex = texture.Clone();

            if (OwnBlock.TextureCodesForOverlays.Contains(textureCode))
            {
                foreach (BannerLayer layer in properties.Patterns.GetOrdered(textureCode))
                {
                    ApplyOverlay(api, textureCode, ctex, layer);
                }

                foreach (BannerLayer layer in properties.Cutouts.GetOrdered(textureCode))
                {
                    ApplyOverlay(api, textureCode, ctex, layer, EnumColorBlendMode.OverlayCutout);
                }
            }

            ctex.Bake(api.Assets);
            texSource.textures[textureCode] = ctex;
        }
        return texSource;
    }

    public void ApplyOverlay(ICoreAPI api, string textureCode, CompositeTexture ctex, BannerLayer layer, EnumColorBlendMode blendMode = EnumColorBlendMode.Normal)
    {
        if ((OwnBlock.IgnoredTextureCodes.TryGetValue(textureCode, out List<string> ignoredTextureCodes) && ignoredTextureCodes.Contains(layer.Pattern)) == true)
        {
            return;
        }
        ctex.BlendedOverlays ??= Array.Empty<BlendedOverlayTexture>();
        if (!OwnBlock.CustomTextures.TryGetValue(layer.TextureCode, out CompositeTexture _overlayTexture) || _overlayTexture == null)
        {
            api.Logger.Error("[Flags] Item {0} defines an overlay texture key '{1}', but no matching texture found", Code, layer.TextureCode);
            ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = AssetLocation.Create(textureUnknown), BlendMode = blendMode });
            return;
        }

        CompositeTexture overlayTexture = _overlayTexture.Clone();
        overlayTexture.FillPlaceholder(textureCodeColor, layer.Color ?? "black");
        overlayTexture.FillPlaceholder(textureCodePattern, layer.Pattern);

        AssetLocation logCode = overlayTexture.Base.Clone().WithPathPrefixOnce(prefixTextures).WithPathAppendixOnce(appendixPng);
        if (!api.Assets.Exists(logCode))
        {
            api.Logger.Error("[Flags] Item {0} defines an overlay texture key '{1}' with path '{2}' for color '{3}', but no matching texture found", Code, layer.TextureCode, logCode.ToString(), layer.Color ?? "black");
            ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = AssetLocation.Create(textureUnknown), BlendMode = blendMode });
            return;
        }

        ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = overlayTexture.Base, BlendMode = blendMode });
    }

    bool IAttachableToEntity.IsAttachable(Entity toEntity, ItemStack itemStack)
    {
        return toEntity is EntityPlayer;
    }

    void IAttachableToEntity.CollectTextures(ItemStack stack, Shape shape, string texturePrefixCode, Dictionary<string, CompositeTexture> intoDict)
    {
        BannerProperties properties = BannerProperties.FromStack(stack);

        foreach ((string textureCode, CompositeTexture texture) in Textures)
        {
            shape.Textures[textureCode] = texture.Baked.BakedName;
        }

        foreach ((string textureCode, CompositeTexture texture) in OwnBlock.CustomTextures)
        {
            CompositeTexture ctex = texture.Clone();

            if (OwnBlock.TextureCodesForOverlays.Contains(textureCode))
            {
                foreach (BannerLayer layer in properties.Patterns.GetOrdered(textureCode))
                {
                    ApplyOverlay(api, textureCode, ctex, layer);
                }

                foreach (BannerLayer layer in properties.Cutouts.GetOrdered(textureCode))
                {
                    ApplyOverlay(api, textureCode, ctex, layer, EnumColorBlendMode.OverlayCutout);
                }
            }

            ctex.Bake(api.Assets);
            intoDict[textureCode] = ctex;
            shape.Textures[textureCode] = ctex.Baked.BakedName;
        }
    }

    string IAttachableToEntity.GetCategoryCode(ItemStack stack) => "";
    CompositeShape IAttachableToEntity.GetAttachedShape(ItemStack stack, string slotCode) => null;
    string[] IAttachableToEntity.GetDisableElements(ItemStack stack) => stack?.Collectible?.Attributes?["disableElements"]?.AsArray<string>();
    string[] IAttachableToEntity.GetKeepElements(ItemStack stack) => null;
    string IAttachableToEntity.GetTexturePrefixCode(ItemStack stack) => GetMeshCacheKey(stack);
    int IAttachableToEntity.RequiresBehindSlots { get; set; }

    Shape IWearableShapeSupplier.GetShape(ItemStack stack, Entity forEntity, string texturePrefixCode)
    {
        Shape shape = GetShapeFromAttributes(stack);
        shape.SubclassForStepParenting(texturePrefixCode);
        return shape;
    }

    private Shape GetShapeFromAttributes(ItemStack itemstack)
    {
        BannerProperties properties = BannerProperties.FromStack(itemstack);
        properties.SetPlacement("player");

        if (OwnBlock.CustomShapes.TryGetValue(properties.Placement, out CompositeShape rcshape))
        {
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Shape shape = api.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            return shape;
        }

        api.Logger.Error("[Flags] No matching shape found for block {0} for type {1}", Code, properties.Placement);
        Shape _shape = api.Assets.TryGet(Shape.Base)?.ToObject<Shape>();
        return _shape;
    }
}
