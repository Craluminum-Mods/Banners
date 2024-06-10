using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Flags;

public class Patterns
{
    public Dictionary<string, string> Elements { get; protected set; } = new();

    public string BaseColor => GetOrdered()?.FirstOrDefault()?.Color;

    public IOrderedEnumerable<BannerLayer> GetOrdered(string textureCode = null)
    {
        return Elements
            .Select(x => BannerLayer.FromLayer(x.Key).WithColor(x.Value).WithTextureCode(textureCode))
            .OrderBy(x => x.Priority.ToInt());
    }

    public bool TryAdd(BannerLayer layer, IWorldAccessor world, IPlayer player = null)
    {
        if (GetLayersLimit(world) + 1 <= Elements.Count && !player.IsCreative())
        {
            return false;
        }
        return Elements.TryAdd(layer.AttributeKey(Elements.Count.ToString()), layer.AttributeValue());
    }

    public bool TryRemoveLast()
    {
        return Elements.Count > 1 && Elements.Remove(GetOrdered().Last().AttributeKey());
    }

    public static int GetLayersLimit(IWorldAccessor world)
    {
        return world.Config.GetAsInt(worldConfigLayersLimit);
    }

    public void GetDescription(StringBuilder dsc, bool withDebugInfo = false)
    {
        if (!Elements.Any())
        {
            return;
        }

        dsc.AppendLine(langCodePatterns.Localize());
        IOrderedEnumerable<BannerLayer> patterns = GetOrdered();
        dsc.Append('\t');
        dsc.AppendLine(patterns.First().Name);
        if (lastPatternDisplayAmount < patterns.Skip(1).Count())
        {
            dsc.AppendLine("...");
        }
        foreach (BannerLayer pattern in patterns.Skip(1).TakeLast(lastPatternDisplayAmount))
        {
            if (withDebugInfo) dsc.Append(pattern).Append('\t');
            dsc.Append('\t');
            dsc.AppendLine(pattern.Name);
        }
    }

    public bool CopyTo(ItemStack toStack)
    {
        BannerProperties toProps = BannerProperties.FromStack(toStack);
        if (Elements.Count > 1 && toProps.Patterns.Elements.Count == 1 && SameBaseColors(toProps))
        {
            ToTreeAttribute(BannerProperties.GetBannerTree(toStack.Attributes));
            return true;
        }
        return false;
    }

    public bool CopyFrom(ItemStack fromStack)
    {
        BannerProperties fromProps = BannerProperties.FromStack(fromStack);
        if (fromProps.Patterns.Elements.Count > 1 && Elements.Count == 1 && SameBaseColors(fromProps))
        {
            FromTreeAttribute(BannerProperties.GetBannerTree(fromStack.Attributes));
            return true;
        }
        return false;
    }

    public bool SameBaseColors(BannerProperties properties)
    {
        return BaseColor == properties.Patterns.BaseColor;
    }

    public void FromTreeAttribute(ITreeAttribute bannerTree)
    {
        ITreeAttribute patternsTree = bannerTree.GetOrAddTreeAttribute(attributeLayers);

        foreach (string key in patternsTree.Select(x => x.Key).Where(key => !Elements.ContainsKey(key)))
        {
            Elements.Add(key, patternsTree.GetString(key));
        }
    }

    public void ToTreeAttribute(ITreeAttribute bannerTree)
    {
        ITreeAttribute patternsTree = bannerTree.GetOrAddTreeAttribute(attributeLayers);

        foreach ((string key, string val) in Elements)
        {
            patternsTree.SetString(key, val);
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();

        if (Elements.Any())
        {
            result.Append('-');
            result.Append('(');
            result.Append(string.Join(layerSeparator, Elements.Select(x => $"{x.Key}-{x.Value}")));
            result.Append(')');
        }
        return result.ToString();
    }
}