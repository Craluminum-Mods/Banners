using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Flags;

public class ItemBannerPattern : ItemRollableFixed
{
    public List<string> PatternGroups { get; protected set; } = new();

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
        PatternGroups.Clear();
        CustomTextures.Clear();
        TextureCodesForOverlays.Clear();
    }

    public void LoadTypes()
    {
        PatternGroups = Attributes[attributePatternGroups].AsObject<List<string>>();

        DefaultType = Attributes[attributeDefaultType].AsString();
        CustomTextures = Attributes[attributeTextures].AsObject<Dictionary<string, CompositeTexture>>();
        TextureCodesForOverlays = Attributes[attributeTextureCodesForOverlays].AsObject<List<string>>();
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder sb, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, sb, world, withDebugInfo);
        sb.AppendLine(langCodePatternGroups.Localize(string.Join(commaSeparator, PatternGroups.Select(group => group))));
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        this.GetInventoryMesh(capi, itemstack, renderinfo);
    }
}