using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Flags.ToolModes.Banner;

public class BannerToolMode
{
    public SetAttribute SetAttribute { get; set; } = new();
    public string Icon { get; set; }
    public string Color { get; set; }
    public bool Linebreak { get; set; }

    public string Name => BannerModes.LangCode(SetAttribute.Key, SetAttribute.Value);

    public AssetLocation Code => AssetLocation.Create(Name);

    public SkillItem GetToolMode(ICoreClientAPI capi, ItemSlot slot)
    {
        SkillItem skillItem = new SkillItem()
        {
            Code = AssetLocation.Create(Name),
            Name = Name.LocalizeM(),
            Linebreak = Linebreak,
            TexturePremultipliedAlpha = !string.IsNullOrEmpty(Color)
        };

        return string.IsNullOrEmpty(Icon) || capi.Assets.TryGet(AssetLocation.Create(Icon)) == null
            ? skillItem
            : skillItem.WithIcon(capi, capi.Gui.LoadSvgWithPadding(AssetLocation.Create(Icon), 48, 48, 5, color: GetColor(slot)));
    }

    public bool IconExists(ICoreClientAPI capi)
    {
        return !string.IsNullOrEmpty(Icon) && capi.Assets.Exists(AssetLocation.Create(Icon));
    }

    public int? GetColor(ItemSlot slot)
    {
        if (SetAttribute.Matches(slot))
        {
            return string.IsNullOrEmpty(Color) ? null : ColorUtil.Hex2Int(Color);
        }
        else return IntColor.Gray;
    }

    public static SkillItem[] GetToolModes(ICoreClientAPI capi, ItemSlot slot, IEnumerable<BannerToolMode> modes)
    {
        return modes.Select(x => x.GetToolMode(capi, slot)).ToArray();
    }
}

public class SetAttribute
{
    public string Key { get; set; }
    public string Value { get; set; }
    public bool Default { get; set; }

    public bool Matches(ItemSlot slot)
    {
        if (slot?.Itemstack?.Attributes == null || slot.Itemstack.Collectible is not BlockBanner)
        {
            return false;
        }

        if (BannerProperties.FromStack(slot.Itemstack).Modes.TryGetValue(Key, out string currentValue))
        {
            return currentValue == Value;
        }

        currentValue = Default ? Value : null;
        return currentValue == Value;
    }

    public void Set(ItemSlot slot)
    {
        if (slot.Itemstack.Collectible is not BlockBanner)
        {
            return;
        }

        BannerProperties props = BannerProperties.FromStack(slot.Itemstack);
        // props.Modes.SetValue(Key, SetValue);
        props.Modes.SetValue(Key, Value);
        props.ToStack(slot.Itemstack);
    }
}