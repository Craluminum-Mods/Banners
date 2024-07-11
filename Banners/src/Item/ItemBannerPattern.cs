using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Flags;

public class ItemBannerPattern : ItemRollableFixed
{
    public Dictionary<string, List<string>> PatternGroupsBy { get; protected set; } = new();

    public Dictionary<string, CompositeTexture> CustomTextures { get; protected set; } = new();
    public List<string> TextureCodesForOverlays { get; protected set; } = new();

    public Dictionary<string, MeshData> Meshes => ObjectCacheUtil.GetOrCreate(api, cacheKeyItemBannerPatternMeshes, () => new Dictionary<string, MeshData>());
    public Dictionary<string, MultiTextureMeshRef> InvMeshes => ObjectCacheUtil.GetOrCreate(api, cacheKeyItemBannerPatternMeshesInv, () => new Dictionary<string, MultiTextureMeshRef>());

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        LoadTypes();
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        base.OnUnloaded(api);
        PatternGroupsBy.Clear();
        CustomTextures.Clear();
        TextureCodesForOverlays.Clear();

        foreach (MeshData mesh in Meshes.Values)
        {
            mesh.Dispose();
        }
        foreach (MultiTextureMeshRef meshRef in InvMeshes.Values)
        {
            meshRef.Dispose();
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
        MultiTextureMeshRef meshRef = this.GetInventoryMesh(capi, itemstack, renderinfo);
        renderinfo.ModelRef = meshRef;
    }
}