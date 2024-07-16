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
    protected Dictionary<string, string> Elements { get; private set; } = new();

    public int Count => Elements.Count;
    public bool Any => Elements.Any();

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
        return TryAdd(layer);
    }

    public bool TryAdd(BannerLayer layer)
    {
        return layer != null && Elements.TryAdd(layer.AttributeKey(Elements.Count.ToString()), layer.AttributeValue());
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

    public bool CanCopyFrom(ItemStack fromStack)
    {
        BannerProperties fromProps = BannerProperties.FromStack(fromStack);
        return fromProps.Patterns.Count > 1 && Count == 1 && SameBaseColors(fromProps);
    }

    public bool CanCopyTo(ItemStack toStack)
    {
        BannerProperties toProps = BannerProperties.FromStack(toStack);
        return Count > 1 && toProps.Patterns.Count == 1 && SameBaseColors(toProps);
    }

    public void CopyFrom(ItemStack fromStack)
    {
        if (CanCopyFrom(fromStack))
        {
            FromTreeAttribute(fromStack.Attributes.GetTreeAttribute(attributeBanner));
        }
    }

    public void CopyTo(ItemStack toStack)
    {
        if (CanCopyTo(toStack))
        {
            ToTreeAttribute(toStack.Attributes.GetOrAddTreeAttribute(attributeBanner));
        }
    }

    public bool SameBaseColors(BannerProperties properties)
    {
        return BaseColor == properties.Patterns.BaseColor;
    }

    public void FromTreeAttribute(ITreeAttribute bannerTree)
    {
        if (!bannerTree.HasAttribute(attributeLayers) || !bannerTree.GetTreeAttribute(attributeLayers).Any())
        {
            return;
        }

        ITreeAttribute patternsTree = bannerTree.GetTreeAttribute(attributeLayers);

        foreach (string key in patternsTree.Select(x => x.Key).Where(key => !Elements.ContainsKey(key)))
        {
            Elements.Add(key, patternsTree.GetString(key));
        }
    }

    public void ToTreeAttribute(ITreeAttribute bannerTree)
    {
        foreach ((string key, string val) in Elements)
        {
            bannerTree.GetOrAddTreeAttribute(attributeLayers).SetString(key, val);
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        if (Elements.Any())
        {
            result.Append('(');
            result.Append(string.Join(layerSeparator, Elements.Select(x => $"{x.Key}-{x.Value}")));
            result.Append(')');
        }
        return result.ToString();
    }
}