using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Flags;

public class Cutouts
{
    public List<string> Elements { get; protected set; } = new();

    public IOrderedEnumerable<BannerLayer> GetOrdered(string textureCode = null)
    {
        return Elements
            .Select(x => BannerLayer.FromLayer(x).WithTextureCode(textureCode))
            .OrderBy(x => x.Priority.ToInt());
    }

    public bool TryAdd(BannerLayer layer)
    {
        string cutout = layer.AttributeKey(Elements.Count.ToString());
        if (!Elements.Contains(cutout))
        {
            Elements.Add(cutout);
            return true;
        }
        return false;
    }

    public bool TryRemoveLast()
    {
        IOrderedEnumerable<BannerLayer> ordered = GetOrdered();
        if (!ordered.Any())
        {
            return false;
        }
        return Elements.Remove(ordered.Last().AttributeKey());
    }

    public void GetDescription(StringBuilder dsc, bool withDebugInfo = false)
    {
        if (!Elements.Any())
        {
            return;
        }

        dsc.AppendLine(langCodeCutouts.Localize());
        IOrderedEnumerable<BannerLayer> cutouts = GetOrdered();
        if (cutoutsDisplayAmount < cutouts.Skip(1).Count())
        {
            dsc.AppendLine("...");
        }
        foreach (BannerLayer cutout in cutouts.Skip(1).TakeLast(cutoutsDisplayAmount))
        {
            if (withDebugInfo) dsc.Append(cutout).Append('\t');
            dsc.Append('\t');
            dsc.AppendLine(cutout.LocalizedPattern);
        }
    }

    // public bool CopyTo(ItemStack toStack)
    // {
    //     BannerProperties toProps = BannerProperties.FromStack(toStack);
    //     if (Elements.Count > 1 && !toProps.Cutouts.Elements.Any())
    //     {
    //         ToTreeAttribute(BannerProperties.GetBannerTree(toStack.Attributes));
    //         return true;
    //     }
    //     return false;
    // }

    // public bool CopyFrom(ItemStack fromStack)
    // {
    //     BannerProperties fromProps = BannerProperties.FromStack(fromStack);
    //     if (fromProps.Cutouts.Elements.Count > 1 && !Elements.Any())
    //     {
    //         FromTreeAttribute(BannerProperties.GetBannerTree(fromStack.Attributes));
    //         return true;
    //     }
    //     return false;
    // }

    public void FromTreeAttribute(ITreeAttribute bannerTree)
    {
        ITreeAttribute cutoutsTree = bannerTree.GetOrAddTreeAttribute(attributeCutouts);

        foreach (string key in cutoutsTree.Select(x => x.Key).Where(x => !Elements.Contains(x)))
        {
            Elements.Add(key);
        }
    }

    public void ToTreeAttribute(ITreeAttribute bannerTree)
    {
        ITreeAttribute cutoutsTree = bannerTree.GetOrAddTreeAttribute(attributeCutouts);

        foreach (string key in Elements)
        {
            cutoutsTree.SetString(key, null);
        }
    }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();
        if (Elements.Any())
        {
            result.Append('[');
            result.Append(string.Join(layerSeparator, Elements));
            result.Append(']');
        }
        return result.ToString();
    }
}