using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Flags;

public class ItemBannerPattern : ItemRollableFixed
{
    public string DefaultType { get; protected set; }
    public Dictionary<string, CompositeTexture> CustomTextures { get; protected set; } = new();
    public List<string> TextureCodesForOverlays { get; protected set; } = new();

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        LoadTypes();
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        base.OnUnloaded(api);
        CustomTextures.Clear();
        TextureCodesForOverlays.Clear();
    }

    public void LoadTypes()
    {
        DefaultType = Attributes[attributeDefaultType].AsString();
        CustomTextures = Attributes[attributeTextures].AsObject<Dictionary<string, CompositeTexture>>();
        TextureCodesForOverlays = Attributes[attributeTextureCodesForOverlays].AsObject<List<string>>();
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        this.GetInventoryMesh(capi, itemstack, renderinfo);
    }
}