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

    public SkillItem GetToolMode(ICoreClientAPI capi)
    {
        SkillItem toolMode = new SkillItem()
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
                toolMode.TexturePremultipliedAlpha = true;
            }
            else
            {
                toolMode.TexturePremultipliedAlpha = false;
            }
            toolMode.WithIcon(capi, capi.Gui.LoadSvgWithPadding(AssetLocation.Create(Icon), 48, 48, 5, color));
        }
        return toolMode;
    }

    public static SkillItem[] GetToolModes(ICoreClientAPI capi, ItemSlot slot, IEnumerable<BannerToolMode> toolModes)
    {
        return toolModes.Where(x => x.Condition.Matches(slot)).Select(x => x.GetToolMode(capi)).ToArray();
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
        if (slot?.Itemstack?.Attributes == null)
        {
            return false;
        }

        string currentValue = slot.Itemstack.Attributes.GetOrAddTreeAttribute(attributeToolModes).GetString(Key, Default ? IsValue : null);
        return currentValue == IsValue;
    }

    public void SetAttribute(ItemSlot slot)
    {
        slot.Itemstack.Attributes.GetOrAddTreeAttribute(attributeToolModes).SetString(Key, SetValue);
    }
}