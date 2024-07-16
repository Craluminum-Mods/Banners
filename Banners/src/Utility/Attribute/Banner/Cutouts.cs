using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Flags;

public class Cutouts
{
    protected List<string> Elements { get; private set; } = new();

    public int Count => Elements.Count;
    public bool Any => Elements.Any();

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
        if (cutoutsDisplayAmount < cutouts.Count())
        {
            dsc.AppendLine("...");
        }
        foreach (BannerLayer cutout in cutouts.TakeLast(cutoutsDisplayAmount))
        {
            if (withDebugInfo) dsc.Append(cutout).Append('\t');
            dsc.Append('\t');
            dsc.AppendLine(cutout.LocalizedPattern);
        }
    }

    public void CopyFrom(ItemStack fromStack)
    {
        FromTreeAttribute(fromStack.Attributes.GetTreeAttribute(attributeBanner));
    }

    public void CopyTo(ItemStack toStack)
    {
        ToTreeAttribute(toStack.Attributes.GetOrAddTreeAttribute(attributeBanner));
    }

    public void FromTreeAttribute(ITreeAttribute bannerTree)
    {
        if (!bannerTree.HasAttribute(attributeCutouts) || !bannerTree.GetTreeAttribute(attributeCutouts).Any())
        {
            return;
        }
        Elements.AddRange(bannerTree.GetTreeAttribute(attributeCutouts).Select(x => x.Key).Where(key => !Elements.Contains(key)));
    }

    public void ToTreeAttribute(ITreeAttribute bannerTree)
    {
        foreach (string key in Elements)
        {
            bannerTree.GetOrAddTreeAttribute(attributeCutouts).SetString(key, "");
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