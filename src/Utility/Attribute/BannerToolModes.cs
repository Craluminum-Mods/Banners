using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vintagestory.API.Datastructures;

namespace Flags;

public class BannerToolModes
{
    public Dictionary<string, string> Elements { get; protected set; } = new();

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

    /// <summary>
    /// For debug purposes only
    /// </summary>
    /// <param name="dsc"></param>
    /// <param name="withDebugInfo"></param>
    public void GetDescription(StringBuilder dsc, bool withDebugInfo = false)
    {
        if (!withDebugInfo)
        {
            return;
        }

        dsc.AppendLine("debug-tool-modes:");
        if (!Elements.Any())
        {
            dsc.AppendLine("nothing...");
        }
        foreach ((string key, string value) in Elements)
        {
            dsc.Append('\t');
            dsc.AppendLine($"{key}: {value}");
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