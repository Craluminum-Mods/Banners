using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Flags;

public class BlockBanner : Block, IContainedMeshSource
{
    public List<string> PatternGroups { get; protected set; } = new();

    public Dictionary<string, CompositeShape> CustomShapes { get; protected set; } = new();
    public Dictionary<string, CompositeShape> CustomShapesContainable { get; protected set; } = new();
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
    public ModelTransform BannerOnBoatTransform { get; protected set; } = new();

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
        CustomShapesContainable.Clear();
        CustomTextures.Clear();
        IgnoredTextureCodes.Clear();
        TextureCodesForOverlays.Clear();
        Colors.Clear();
        IgnoreForGeneratingTextures.Clear();
        CustomSelectionBoxes.Clear();
        CustomCollisionBoxes.Clear();

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
        CustomShapesContainable = Attributes[attributeShapesContainable].AsObject<Dictionary<string, CompositeShape>>();
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

        EnumHandling handling = EnumHandling.PassThrough;
        if (api is ICoreClientAPI capi)
        {
            capi.Event.RegisterEventBusListener((string eventName, ref EnumHandling handling, IAttribute data) => {
                switch (eventName)
                {
                    case eventOnCloseEditTransforms:
                    case eventOnEditTransforms:
                    case eventOnApplyTransforms:
                    case eventGenJsonTransform:
                        LoadTransforms();
                        break;
                }
            });
        }
    }

    public void LoadTransforms()
    {
        BannerPreviewHudTransform = Attributes[attributeBannerPreviewHudTransform].AsObject<ModelTransform>();
        BannerOnBoatTransform = Attributes[attributeBannerOnBoatTransform].AsObject<ModelTransform>();
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

        BannerProperties.GetBannerTree(handbookStack.Attributes).RemoveAttribute(attributeName);
        BannerProperties.GetBannerTree(handbookStack.Attributes).RemoveAttribute(attributeCutouts);
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
            if (blockSel.Face.IsHorizontal)
            {
                BannerProperties.SetPlacement(byItemStack.Attributes, DefaultHorizontalPlacement);
            }
            else if (blockSel.Face.IsVertical)
            {
                BannerProperties.SetPlacement(byItemStack.Attributes, DefaultVerticalPlacement);
            }
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
        return this.GetOrCreateMesh(api as ICoreClientAPI, props);
    }

    public string GetMeshCacheKey(ItemStack itemstack)
    {
        BannerProperties props = BannerProperties.FromStack(itemstack);
        props.SetPlacement(DefaultVerticalPlacement);
        return $"{itemstack.Collectible.Code}{props}";
    }
}