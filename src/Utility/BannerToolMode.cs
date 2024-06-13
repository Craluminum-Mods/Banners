using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Flags.ToolModes.Banner;

public class BannerToolMode
{
    public Condition Condition { get; set; } = new();
    public string Icon { get; set; }
    public string Color { get; set; }
    public bool Linebreak { get; set; }

    public string Name => $"{langCodeToolMode}{Condition.Key}-{Condition.IsValue}-{Condition.SetValue}";
    public AssetLocation Code => AssetLocation.Create(Name);

    public SkillItem GetToolMode(ICoreClientAPI capi, ItemSlot slot)
    {
        if (!Condition.Matches(slot))
        {
            return new SkillItem()
            {
                Enabled = false,
                Code = AssetLocation.Create("skillitem-dummy"),
                Name = "",
                Linebreak = Linebreak
            };
        }

        SkillItem skillItem = new SkillItem()
        {
            Code = AssetLocation.Create(Name),
            Name = Name.LocalizeM(),
            Linebreak = Linebreak,
        };

        if (!string.IsNullOrEmpty(Icon) && capi.Assets.TryGet(AssetLocation.Create(Icon)) != null)
        {
            int? color = null;
            if (!string.IsNullOrEmpty(Color))
            {
                color = ColorUtil.Hex2Int(Color);
                skillItem.TexturePremultipliedAlpha = true;
            }
            else
            {
                skillItem.TexturePremultipliedAlpha = false;
            }
            skillItem.WithIcon(capi, capi.Gui.LoadSvgWithPadding(AssetLocation.Create(Icon), 48, 48, 5, color));
        }
        return skillItem;
    }

    public static SkillItem[] GetToolModes(ICoreClientAPI capi, ItemSlot slot, IEnumerable<BannerToolMode> modes)
    {
        return modes.Select(x => x.GetToolMode(capi, slot)).ToArray();
    }
}

public class Condition
{
    public string Key { get; set; }
    public string IsValue { get; set; }
    public string SetValue { get; set; }
    public bool Default { get; set; }

    public bool Matches(ItemSlot slot)
    {
        if (slot?.Itemstack?.Attributes == null || slot.Itemstack.Collectible is not BlockBanner)
        {
            return false;
        }

        if (BannerProperties.FromStack(slot.Itemstack).Modes.TryGetValue(Key, out string currentValue))
        {
            return currentValue == IsValue;
        }

        currentValue = Default ? IsValue : null;
        return currentValue == IsValue;
    }

    public void SetAttribute(ItemSlot slot)
    {
        if (slot.Itemstack.Collectible is not BlockBanner)
        {
            return;
        }

        BannerProperties props = BannerProperties.FromStack(slot.Itemstack);
        props.Modes.SetValue(Key, SetValue);
        props.ToStack(slot.Itemstack);
    }
}