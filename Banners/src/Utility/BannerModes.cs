using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vintagestory.API.Datastructures;

namespace Flags;

public class BannerModes
{
    protected Dictionary<string, string> Elements { get; private set; } = new();

    public int Count => Elements.Count;
    public bool Any => Elements.Any();

    public bool this[BannerMode mode] => TryGetValue(mode.Key, out string value) && value == mode.Value;
    public string this[string key] => TryGetValue(key, out string value) ? value : "";

    public static string LangCode(string key, string value) => $"{langCodeToolMode}{key}-{value}";

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
            if ($"{langCodeToolMode}{key}".HasTranslation())
            {
                if (withDebugInfo) dsc.Append($"{key}: {value}").Append('\t');
                dsc.Append('\t');
                dsc.AppendLine($"{langCodeToolMode}{key}".Localize($"{langCodeToolModeValue}{value}".Localize()));
            }
        }
    }

    /// <param name="mainTree">The main attribute tree that is not a “banner”</param>
    public void FromTreeAttribute(ITreeAttribute mainTree, Dictionary<string, string> defaultValues)
    {
        if (mainTree == null && defaultValues != null && defaultValues.Any())
        {
            Elements = defaultValues;
            return;
        }

        if (mainTree == null || mainTree?.HasAttribute(attributeBannerModes) == false || mainTree?.GetTreeAttribute(attributeBannerModes).Any() == false || defaultValues == null || !defaultValues.Any())
        {
            return;
        }

        ITreeAttribute modesTree = mainTree.GetOrAddTreeAttribute(attributeBannerModes);
        
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
            mainTree.GetOrAddTreeAttribute(attributeBannerModes).SetString(key, value);
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();

        if (Elements.Any())
        {
            result.Append(';');
            result.Append(string.Join(layerSeparator, Elements.Select(x => $"{x.Key}-{x.Value}")));
            result.Append(';');
        }
        return result.ToString();
    }
}

public struct BannerMode
{
    // public static readonly string DisplayOnMap = "displayonmap";
    public const string PickUp = "pickup";
    public const string Wind = "wind";
    public const string EditMode = "editmode";
    public const string SaveRotations = "saverotations";

    // public static readonly BannerMode DisplayOnMap_On = new BannerMode(DisplayOnMap, "on");
    // public static readonly BannerMode DisplayOnMap_Off = new BannerMode(DisplayOnMap, "off");
    // public static readonly BannerMode DisplayOnMap_Group = new BannerMode(DisplayOnMap, "group");
    public static readonly BannerMode PickUp_On = new BannerMode(PickUp, "on");
    public static readonly BannerMode PickUp_Off = new BannerMode(PickUp, "off");
    public static readonly BannerMode Wind_On = new BannerMode(Wind, "on");
    public static readonly BannerMode Wind_Off = new BannerMode(Wind, "off");
    public static readonly BannerMode EditMode_On = new BannerMode(EditMode, "on");
    public static readonly BannerMode EditMode_Off = new BannerMode(EditMode, "off");
    public static readonly BannerMode SaveRotations_On = new BannerMode(SaveRotations, "on");
    public static readonly BannerMode SaveRotations_Off = new BannerMode(SaveRotations, "off");

    public string Key { get; private set; }
    public string Value { get; private set; }

    public BannerMode(string key, string value)
    {
        Key = key;
        Value = value;
    }
}