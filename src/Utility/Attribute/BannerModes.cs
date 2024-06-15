using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vintagestory.API.Datastructures;

namespace Flags;

public class BannerModes
{
    protected Dictionary<string, string> Elements { get; private set; } = new();

    public bool this[BannerMode mode] => TryGetValue(mode.Key, out string value) && value == mode.Value;

    public string LangCode(string key)
    {
        return TryGetValue(key, out string value) ? $"{langCodeToolMode}{key}-{value}" : "";
    }

    public string ErrorCode(string key)
    {
        return TryGetValue(key, out string value) ? $"{IngameError.Prefix}{key}-{value}" : "";
    }

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
            if (!LangCode(key).HasTranslation())
            {
                continue;
            }
            if (withDebugInfo) dsc.Append($"{key}: {value}").Append('\t');
            dsc.Append('\t');
            dsc.AppendLine(LangCode(key).Localize());
        }
    }

    /// <param name="mainTree">The main attribute tree that is not a “banner”</param>
    public void FromTreeAttribute(ITreeAttribute mainTree, Dictionary<string, string> defaultValues)
    {
        ITreeAttribute modesTree = GetModes(mainTree);
        foreach (string key in modesTree.Select(x => x.Key).Where(key => !Exists(key)))
        {
            SetValue(key, modesTree.GetString(key));
        }
        foreach ((string key, string value) in defaultValues.Where(key => !Exists(key.Key)))
        {
            SetValue(key, value);
        }
    }

    /// <param name="mainTree">The main attribute tree that is not a “banner”</param>
    public void ToTreeAttribute(ITreeAttribute mainTree)
    {
        foreach ((string key, string value) in Elements)
        {
            GetModes(mainTree).SetString(key, value);
        }
    }

    public static ITreeAttribute GetModes(ITreeAttribute tree) => tree.GetOrAddTreeAttribute(attributeBannerModes);

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
    public static readonly BannerMode Wind_Off = new BannerMode("wind", "off");
    // public static readonly BannerMode Axis_Free = new BannerMode("axis", "free");
    // public static readonly BannerMode Axis_Lock = new BannerMode("axis", "lock");
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