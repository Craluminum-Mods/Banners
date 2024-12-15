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

namespace Flags;

public class BlockBanner : Block, IContainedMeshSource, IAttachableToEntity, IWearableShapeSupplier
{
    public List<string> PatternGroups { get; protected set; } = new();

    public Dictionary<string, CompositeShape> CustomShapes { get; protected set; } = new();
    public Dictionary<string, CompositeTexture> CustomTextures { get; protected set; } = new();
    public Dictionary<string, List<string>> IgnoredTextureCodes { get; protected set; } = new();
    public List<string> TextureCodesForOverlays { get; protected set; } = new();

    public Dictionary<string, Cuboidf[]> CustomSelectionBoxes { get; protected set; } = new();
    public Dictionary<string, Cuboidf[]> CustomCollisionBoxes { get; protected set; } = new();

    public string DefaultPlacement { get; protected set; }
    public string DefaultHorizontalPlacement { get; protected set; }
    public string DefaultVerticalPlacement { get; protected set; }

    public bool ShowDebugInfo { get; protected set; }

    public string TopTexturePrefix { get; protected set; }
    public List<string> Colors { get; protected set; } = new();
    public List<string> IgnoreForGeneratingTextures { get; protected set; } = new();

    public Dictionary<string, string> DefaultModes { get; protected set; } = new();

    public ModelTransform BannerPreviewHudTransform { get; protected set; } = new();

    public Dictionary<string, MeshData> Meshes => ObjectCacheUtil.GetOrCreate(api, cacheKeyBlockBannerMeshes, () => new Dictionary<string, MeshData>());
    public Dictionary<string, MeshData> ContainableMeshes => ObjectCacheUtil.GetOrCreate(api, cacheKeyBlockBannerContainableMeshes, () => new Dictionary<string, MeshData>());
    public Dictionary<string, MultiTextureMeshRef> InvMeshes => ObjectCacheUtil.GetOrCreate(api, cacheKeyBlockBannerInvMeshes, () => new Dictionary<string, MultiTextureMeshRef>());

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        LoadTypes();
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        base.OnUnloaded(api);
        PatternGroups.Clear();
        CustomShapes.Clear();
        CustomTextures.Clear();
        IgnoredTextureCodes.Clear();
        TextureCodesForOverlays.Clear();
        Colors.Clear();
        IgnoreForGeneratingTextures.Clear();
        CustomSelectionBoxes.Clear();
        CustomCollisionBoxes.Clear();
        DefaultModes.Clear();

        foreach (MeshData mesh in Meshes.Values)
        {
            mesh.Dispose();
        }
        foreach (MeshData mesh in ContainableMeshes.Values)
        {
            mesh.Dispose();
        }
        foreach (MultiTextureMeshRef meshRef in InvMeshes.Values)
        {
            meshRef.Dispose();
        }

        ObjectCacheUtil.Delete(api, cacheKeyBlockBannerMeshes);
        ObjectCacheUtil.Delete(api, cacheKeyBlockBannerContainableMeshes);
        ObjectCacheUtil.Delete(api, cacheKeyBlockBannerInvMeshes);
    }

    public void LoadTypes()
    {
        PatternGroups = Attributes[attributePatternGroups].AsObject<List<string>>();

        CustomShapes = Attributes[attributeShapes].AsObject<Dictionary<string, CompositeShape>>();
        CustomTextures = Attributes[attributeTextures].AsObject<Dictionary<string, CompositeTexture>>();
        IgnoredTextureCodes = Attributes[attributeIgnoredTextureCodesForOverlays].AsObject<Dictionary<string, List<string>>>();
        TextureCodesForOverlays = Attributes[attributeTextureCodesForOverlays].AsObject<List<string>>();

        CustomSelectionBoxes = Attributes[attributeSelectionBoxes].AsObject<Dictionary<string, Cuboidf[]>>();
        CustomCollisionBoxes = Attributes[attributeCollisionBoxes].AsObject<Dictionary<string, Cuboidf[]>>();

        DefaultPlacement = Attributes[attributeDefaultPlacement].AsString();
        DefaultHorizontalPlacement = Attributes[attributeDefaultHorizontalPlacement].AsString();
        DefaultVerticalPlacement = Attributes[attributeDefaultVerticalPlacement].AsString();

        ShowDebugInfo = Attributes[attributeShowDebugInfo].AsBool();

        TopTexturePrefix = Attributes[attributeTopTexturePrefix].AsString();
        Colors = Attributes[attributeColors].AsObject<List<string>>();
        IgnoreForGeneratingTextures = Attributes[attributeIgnoredTextureCodesForGeneratingTextures].AsObject<List<string>>();

        DefaultModes = Attributes[attributeDefaultModes].AsObject<Dictionary<string, string>>();

        LoadTransforms();

        if (api is not ICoreClientAPI capi)
        {
            return;
        }
        capi.Event.RegisterEventBusListener(OnEvent);
    }

    private void OnEvent(string eventName, ref EnumHandling handling, IAttribute data)
    {
        switch (eventName)
        {
            case eventOnEditTransforms:
            case eventOnApplyTransforms:
            case eventGenJsonTransform:
            case eventOnCloseEditTransforms:
                LoadTransforms();
                break;
        }
    }

    public void LoadTransforms()
    {
        BannerPreviewHudTransform = Attributes[attributeBannerPreviewHudTransform].AsObject<ModelTransform>();
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder sb, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, sb, world, withDebugInfo);
        BannerProperties.FromStack(inSlot.Itemstack).GetDescription(this, (world as IClientWorldAccessor)?.Player, sb, ShowDebugInfo);
    }

    public override bool Equals(ItemStack thisStack, ItemStack otherStack, params string[] ignoreAttributeSubTrees)
    {
        ignoreAttributeSubTrees ??= Array.Empty<string>();
        ignoreAttributeSubTrees = ignoreAttributeSubTrees.Append(BannersIgnoreAttributeSubTrees);
        return base.Equals(thisStack, otherStack, ignoreAttributeSubTrees);
    }

    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        return new ItemStack[1] { OnPickBlock(world, pos) };
    }

    public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
    {
        BlockDropItemStack[] drops = base.GetDropsForHandbook(handbookStack, forPlayer);

        handbookStack.Attributes.GetTreeAttribute(attributeBanner)?.RemoveAttribute(attributeName);
        handbookStack.Attributes.GetTreeAttribute(attributeBanner)?.RemoveAttribute(attributeCutouts);
        handbookStack.Attributes.RemoveAttribute(attributeBannerModes);
        handbookStack.Attributes.RemoveAttribute(attributeRotX);
        handbookStack.Attributes.RemoveAttribute(attributeRotY);
        handbookStack.Attributes.RemoveAttribute(attributeRotZ);

        drops[0] = drops[0].Clone();
        drops[0].ResolvedItemstack.SetFrom(handbookStack);
        return drops;
    }

    public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return blockAccessor.GetBlockEntity(pos) is BlockEntityBanner be ? be.GetOrCreateSelectionBoxes() : base.GetSelectionBoxes(blockAccessor, pos);
    }

    public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
    {
        return blockAccessor.GetBlockEntity(pos) is BlockEntityBanner be ? be.GetOrCreateCollisionBoxes() : base.GetCollisionBoxes(blockAccessor, pos);
    }

    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
    {
        bool place = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
        if (place && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityBanner be)
        {
            IRotatableBanner rotatableBanner = GetInterface<IRotatableBanner>(world, blockSel.Position);
            rotatableBanner?.RotateWhenPlaced(byPlayer, blockSel, byItemStack, be);
            BannerProperties.SetPlacement(byItemStack.Attributes, blockSel.Face.IsHorizontal ? DefaultHorizontalPlacement : DefaultVerticalPlacement);
            be.OnBlockPlaced(byItemStack);
        }
        return place;
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        ItemStack stack = base.OnPickBlock(world, pos);
        if (world.BlockAccessor.GetBlockEntity(pos) is not BlockEntityBanner be)
        {
            return stack;
        }

        be.BannerProps.ToStack(stack);

        IRotatableBanner rotatableBanner = GetInterface<IRotatableBanner>(world, pos);
        if (rotatableBanner == null || (rotatableBanner.RotateX == 0 && rotatableBanner.RotateY == 0 && rotatableBanner.RotateZ == 0))
        {
            return stack;
        }

        bool saverotations = be.BannerProps.Modes[BannerMode.SaveRotations_On];
        stack.Attributes.SetFloat(attributeRotX, saverotations ? rotatableBanner.RotateX : 0);
        stack.Attributes.SetFloat(attributeRotY, saverotations ? rotatableBanner.RotateY : 0);
        stack.Attributes.SetFloat(attributeRotZ, saverotations ? rotatableBanner.RotateZ : 0);
        return stack;
    }

    public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
    {
        if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityBanner be)
        {
            MeshData decalMesh = this.GetOrCreateMesh(api, be.BannerProps, decalTexSource);
            MeshData blockMesh = this.GetOrCreateMesh(api, be.BannerProps);
            IRotatableBanner rotatableBanner = GetInterface<IRotatableBanner>(world, pos);
            decalModelData = rotatableBanner?.RotatedMesh(decalMesh) ?? decalMesh;
            blockModelData = rotatableBanner?.RotatedMesh(blockMesh) ?? blockMesh;
        }
        else
        {
            base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);
        }
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        MultiTextureMeshRef meshRef = this.GetInventoryMesh(capi, itemstack);
        if (itemstack.TempAttributes.GetBool(attributeInBannerPreviewHUD))
        {
            renderinfo.Transform = BannerPreviewHudTransform;
        }

        renderinfo.ModelRef = meshRef;

        if (target == EnumItemRenderTarget.Gui)
        {
            renderinfo.NormalShaded = false;
        }
    }

    public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos)
    {
        BannerProperties props = BannerProperties.FromStack(itemstack);
        props.SetPlacement(DefaultVerticalPlacement);
        return GetOrCreateMesh(api as ICoreClientAPI, props);
    }

    public string GetMeshCacheKey(ItemStack itemstack)
    {
        BannerProperties props = BannerProperties.FromStack(itemstack);
        string compactKey = props.ToCompactString();
        return $"{itemstack.Collectible.Code}-{compactKey}";
    }

    public MeshData GetOrCreateMesh(ICoreAPI api, BannerProperties properties, ITexPositionSource overrideTexturesource = null)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;

        if (string.IsNullOrEmpty(properties.Placement))
        {
            properties.SetPlacement(DefaultPlacement);
        }

        string key = $"{Code}-{properties}";

        if (overrideTexturesource != null || !Meshes.TryGetValue(key, out MeshData mesh))
        {
            if (!CustomShapes.TryGetValue(properties.Placement, out CompositeShape rcshape))
            {
                capi.Tesselator.TesselateBlock(this, out mesh);
                capi.Logger.Error("[Flags] No matching shape found for block {0} for type {1}", Code, properties.Placement);
                return mesh;
            }
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Shape shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            ITexPositionSource texSource = overrideTexturesource ?? HandleTextures(properties, capi, shape, rcshape.Base.ToString());
            if (shape == null)
            {
                capi.Tesselator.TesselateBlock(this, out mesh);
                capi.Logger.Error("[Flags] Block {0} defines shape '{1}', but no matching shape found", Code, rcshape.Base);
                return mesh;
            }
            try
            {
                capi.Tesselator.TesselateShape("Banner block", shape, out mesh, texSource);
            }
            catch (Exception)
            {
                capi.Tesselator.TesselateBlock(this, out mesh);
                capi.Logger.Error("[Flags] Can't create shape for block {0} because of broken textures", Code);
                return mesh;
            }
            if (properties.Modes[BannerMode.Wind_Off])
            {
                mesh.ClearWindFlags();
            }
            if (overrideTexturesource == null)
            {
                Meshes[key] = mesh;
            }
        }
        return mesh;
    }

    public MeshData GetOrCreateContainableMesh(ICoreAPI api, ItemStack stack, string shapeKey, Vec3f rotation)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;

        BannerProperties properties = BannerProperties.FromStack(stack);
        string key = $"{Code}-{properties}-{shapeKey}-{rotation}";

        if (!ContainableMeshes.TryGetValue(key, out MeshData mesh))
        {
            if (!CustomShapes.TryGetValueOrWildcard(shapeKey, out CompositeShape rcshape))
            {
                capi.Tesselator.TesselateBlock(this, out mesh);
                capi.Logger.Error("[Flags] No matching shape found for block {0} for BannerContainable key '{1}'", Code, shapeKey);
                return mesh;
            }
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Shape shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            ITexPositionSource texSource = HandleTextures(properties, capi, shape, rcshape.Base.ToString());
            if (shape == null)
            {
                capi.Tesselator.TesselateBlock(this, out mesh);
                capi.Logger.Error("[Flags] BannerContainable {0} defines shape '{1}', but no matching shape found", Code, rcshape.Base);
                return mesh;
            }
            try
            {
                capi.Tesselator.TesselateShape("Containable banner block", shape, out mesh, texSource, rotation);
            }
            catch (Exception)
            {
                capi.Tesselator.TesselateBlock(this, out mesh);
                capi.Logger.Error("[Flags] Can't create shape for block {0} for BannerContainable key '{1}' because of broken textures", Code, shapeKey);
                return mesh;
            }
            ContainableMeshes[key] = mesh;
        }
        return mesh;
    }

    public MultiTextureMeshRef GetInventoryMesh(ICoreClientAPI capi, ItemStack stack)
    {
        BannerProperties properties = BannerProperties.FromStack(stack);
        string key = $"{Code}-{properties}";
        if (!InvMeshes.TryGetValue(key, out MultiTextureMeshRef meshref))
        {
            MeshData mesh = GetOrCreateMesh(capi, properties);
            meshref = InvMeshes[key] = capi.Render.UploadMultiTextureMesh(mesh);
        }
        return meshref;
    }

    public ITexPositionSource HandleTextures(BannerProperties properties, ICoreAPI api, Shape shape, string filenameForLogging = "")
    {
        ShapeTextureSource texSource = new ShapeTextureSource(api as ICoreClientAPI, shape, filenameForLogging);

        foreach ((string textureCode, CompositeTexture texture) in CustomTextures)
        {
            CompositeTexture ctex = texture.Clone();

            if (TextureCodesForOverlays.Contains(textureCode))
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
        if ((IgnoredTextureCodes.TryGetValue(textureCode, out List<string> ignoredTextureCodes) && ignoredTextureCodes.Contains(layer.Pattern)) == true)
        {
            return;
        }
        ctex.BlendedOverlays ??= Array.Empty<BlendedOverlayTexture>();
        if (!CustomTextures.TryGetValue(layer.TextureCode, out CompositeTexture _overlayTexture) || _overlayTexture == null)
        {
            api.Logger.Error("[Flags] Block {0} defines an overlay texture key '{1}', but no matching texture found", Code, layer.TextureCode);
            ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = AssetLocation.Create(textureUnknown), BlendMode = blendMode });
            return;
        }

        CompositeTexture overlayTexture = _overlayTexture.Clone();
        overlayTexture.FillPlaceholder(textureCodeColor, layer.Color ?? "black");
        overlayTexture.FillPlaceholder(textureCodePattern, layer.Pattern);

        AssetLocation logCode = overlayTexture.Base.Clone().WithPathPrefixOnce(prefixTextures).WithPathAppendixOnce(appendixPng);
        if (!api.Assets.Exists(logCode))
        {
            api.Logger.Error("[Flags] Block {0} defines an overlay texture key '{1}' with path '{2}' for color '{3}', but no matching texture found", Code, layer.TextureCode, logCode.ToString(), layer.Color ?? "black");
            ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = AssetLocation.Create(textureUnknown), BlendMode = blendMode });
            return;
        }

        ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = overlayTexture.Base, BlendMode = blendMode });
    }

    bool IAttachableToEntity.IsAttachable(Entity toEntity, ItemStack itemStack) => true;

    void IAttachableToEntity.CollectTextures(ItemStack stack, Shape shape, string texturePrefixCode, Dictionary<string, CompositeTexture> intoDict)
    {
        BannerProperties properties = BannerProperties.FromStack(stack);

        foreach ((string textureCode, CompositeTexture texture) in CustomTextures)
        {
            CompositeTexture ctex = texture.Clone();

            if (TextureCodesForOverlays.Contains(textureCode))
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

            intoDict[textureCode] = ctex;
        }
    }

    string IAttachableToEntity.GetCategoryCode(ItemStack stack) => "banner";
    CompositeShape IAttachableToEntity.GetAttachedShape(ItemStack stack, string slotCode) => null;
    string[] IAttachableToEntity.GetDisableElements(ItemStack stack) => stack?.Collectible?.Attributes?["disableElements"]?.AsArray<string>();
    string[] IAttachableToEntity.GetKeepElements(ItemStack stack) => null;
    string IAttachableToEntity.GetTexturePrefixCode(ItemStack stack) => GetMeshCacheKey(stack);

    Shape IWearableShapeSupplier.GetShape(ItemStack stack, Entity forEntity, string texturePrefixCode)
    {
        BannerProperties properties = BannerProperties.FromStack(stack);

        if (forEntity == null)
        {
            properties.SetPlacement(DefaultVerticalPlacement);
        }
        else
        {
            string firstKey = CustomShapes.Select(x => x.Key).FirstOrDefault(forEntity.WildCardMatch, defaultValue: DefaultVerticalPlacement);
            properties.SetPlacement(firstKey);
        }

        if (CustomShapes.TryGetValue(properties.Placement, out CompositeShape rcshape))
        {
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Shape shape = api.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            return shape.RemoveWindData();
        }

        api.Logger.Error("[Flags] No matching shape found for block {0} for type {1}", Code, properties.Placement);
        Shape _shape = api.Assets.TryGet(Shape.Base)?.ToObject<Shape>();
        return _shape.RemoveWindData();
    }
}