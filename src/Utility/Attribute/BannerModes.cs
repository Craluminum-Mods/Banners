using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Flags;

public class BannerModes
{
    public Dictionary<string, string> Elements { get; protected set; } = new();

    public bool this[BannerMode mode] => TryGetValue(mode.Key, out string value) && value == mode.Value;

    public bool Exists(string key) => Elements.ContainsKey(key);

    public void SetValue(string key, string value)
    {
        if (Exists(key))
        {
            Elements[key] = value;
        }
        else
        {
            Elements.TryAdd(key, value);
        }
    }

    public bool TryGetValue(string key, out string value)
    {
        return Elements.TryGetValue(key, out value);
    }

    public void GetDescription(StringBuilder dsc, bool withDebugInfo = false)
    {
        if (!Elements.Any())
        {
            return;
        }
        dsc.AppendLine(langCodeBannerModes.Localize());
        foreach ((string key, string value) in Elements)
        {
            if (!Lang.HasTranslation($"{langCodeToolMode}{key}-{value}"))
            {
                continue;
            }
            if (withDebugInfo) dsc.Append($"{key}: {value}").Append('\t');
            dsc.Append('\t');
            dsc.AppendLine($"{langCodeToolMode}{key}-{value}".Localize());
        }
    }

    /// <param name="mainTree">The main attribute tree that is not a “banner”</param>
    public void FromTreeAttribute(ITreeAttribute mainTree, Dictionary<string, string> defaultValues)
    {
        ITreeAttribute toolModesTree = GetToolModes(mainTree);
        foreach (string key in toolModesTree.Select(x => x.Key).Where(key => !Exists(key)))
        {
            SetValue(key, toolModesTree.GetString(key));
        }

        foreach ((string key, string value) in defaultValues)
        {
            if (!Exists(key))
            {
                SetValue(key, value);
            }
        }
    }

    /// <param name="mainTree">The main attribute tree that is not a “banner”</param>
    public void ToTreeAttribute(ITreeAttribute mainTree)
    {
        foreach ((string key, string value) in Elements)
        {
            GetToolModes(mainTree).SetString(key, value);
        }
    }

    public static ITreeAttribute GetToolModes(ITreeAttribute tree) => tree.GetOrAddTreeAttribute(attributeBannerToolModes);

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();

        if (Elements.Any())
        {
            result.Append('-');
            result.Append(';');
            result.Append(string.Join(layerSeparator, Elements.Select(x => $"{x.Key}-{x.Value}")));
            result.Append(';');
        }
        return result.ToString();
    }
}

public struct BannerMode
{
    public static readonly BannerMode DisplayOnMap_On = new BannerMode("displayonmap", "on");
    public static readonly BannerMode DisplayOnMap_Off = new BannerMode("displayonmap", "off");
    public static readonly BannerMode DisplayOnMap_Group = new BannerMode("displayonmap", "group");
    public static readonly BannerMode PickUp_On = new BannerMode("pickup", "on");
    public static readonly BannerMode PickUp_Off = new BannerMode("pickup", "off");
    public static readonly BannerMode Wind_On = new BannerMode("wind", "on");
    public static readonly BannerMode Wind_Of = new BannerMode("wind", "off");
    public static readonly BannerMode Axis_Free = new BannerMode("axis", "free");
    public static readonly BannerMode Axis_Lock = new BannerMode("axis", "lock");
    public static readonly BannerMode EditMode_On = new BannerMode("editmode", "on");
    public static readonly BannerMode EditMode_Off = new BannerMode("editmode", "off");
    public static readonly BannerMode SaveRotations_On = new BannerMode("saverotations", "on");
    public static readonly BannerMode SaveRotations_Off = new BannerMode("saverotations", "off");

    public string Key { get; private set; }
    public string Value { get; private set; }

    public BannerMode(string key, string value)
    {
        Key = key;
        Value = value;
    }
}