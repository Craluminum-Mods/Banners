using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Flags;

public class BannerProperties
{
    public string Name { get; protected set; }
    public string Placement { get; protected set; }
    public Dictionary<string, string> Layers { get; protected set; } = new();

    public string BaseColor => GetOrderedLayers()?.FirstOrDefault()?.Color;

    public BannerProperties(string defaultPlacement)
    {
        if (!string.IsNullOrEmpty(defaultPlacement))
        {
            Placement ??= defaultPlacement;
        }
    }

    public IOrderedEnumerable<BannerLayer> GetOrderedLayers(string textureCode = null)
    {
        return Layers
            .Select(x => new BannerLayer(x.Key, x.Value, textureCode))
            .OrderBy(x => x.Priority.ToInt());
    }

    public bool AddLayer(BannerLayer layer, IWorldAccessor world, IPlayer player = null)
    {
        if (world.Config.GetAsInt(worldConfigLayersLimit) + 1 <= Layers.Count && player?.WorldData.CurrentGameMode != EnumGameMode.Creative)
        {
            return false;
        }
        return Layers.TryAdd(layer.AttributeKey(Layers.Count.ToString()), layer.AttributeValue());
    }

    public bool RemoveLastLayer()
    {
        return Layers.Count > 1 && Layers.Remove(GetOrderedLayers().Last().AttributeKey());
    }

    public void GetDescription(StringBuilder dsc, bool withDebugInfo = false)
    {
        foreach (BannerLayer layer in GetOrderedLayers())
        {
            if (withDebugInfo)
            {
                dsc.Append(layer).Append('\t');
            }
            dsc.AppendLine(layer.Name);
        }
    }

    public BannerProperties FromTreeAttribute(ITreeAttribute tree)
    {
        ITreeAttribute bannerTree = GetBannerTree(tree);
        LayersFromTree(bannerTree);
        Name = bannerTree.GetString(attributeName, Name);
        Placement = bannerTree.GetString(attributePlacement, Placement);
        return this;
    }

    public void ToTreeAttribute(ITreeAttribute tree, bool setPlacement = true)
    {
        ITreeAttribute bannerTree = GetBannerTree(tree);
        LayersToTree(bannerTree);

        if (!string.IsNullOrEmpty(Name))
        {
            bannerTree.SetString(attributeName, Name);
        }
        if (setPlacement)
        {
            SetPlacement(tree, Placement);
        }
    }

    public void LayersFromTree(ITreeAttribute bannerTree)
    {
        ITreeAttribute layersTree = bannerTree.GetOrAddTreeAttribute(attributeLayers);

        foreach (string key in layersTree.Select(x => x.Key).Where(x => !Layers.ContainsKey(x)))
        {
            Layers.Add(key, layersTree.GetString(key));
        }
    }

    public void LayersToTree(ITreeAttribute bannerTree)
    {
        ITreeAttribute layersTree = bannerTree.GetOrAddTreeAttribute(attributeLayers);

        foreach ((string key, string val) in Layers)
        {
            layersTree.SetString(key, val);
        }
    }

    public static BannerProperties FromStack(ItemStack stack, BlockBanner blockBanner = null)
    {
        string defaultType = blockBanner?.DefaultPlacement ?? (stack.Collectible as BlockBanner)?.DefaultPlacement;
        return new BannerProperties(defaultType).FromTreeAttribute(stack.Attributes);
    }

    public void ToStack(ItemStack stack)
    {
        ToTreeAttribute(stack.Attributes, false);
    }

    public bool CopyFrom(ItemStack fromStack, bool copyLayers = false)
    {
        BannerProperties fromProps = FromStack(fromStack);
        if (copyLayers && fromProps.Layers.Count > 1 && Layers.Count == 1 && fromProps.BaseColor == BaseColor)
        {
            LayersFromTree(GetBannerTree(fromStack.Attributes));
            return true;
        }
        return false;
    }

    public bool CopyTo(ItemStack toStack, bool copyLayers = false)
    {
        BannerProperties toProps = FromStack(toStack);
        if (copyLayers && Layers.Count > 1 && toProps.Layers.Count == 1 && toProps.BaseColor == BaseColor)
        {
            LayersToTree(GetBannerTree(toStack.Attributes));
            return true;
        }
        return false;
    }

    public void SetPlacement(string placement)
    {
        Placement = placement;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public static void SetPlacement(ITreeAttribute tree, string placement)
    {
        tree.GetOrAddTreeAttribute(attributeBanner).SetString(attributePlacement, placement);
    }

    public static void ClearPlacement(ITreeAttribute tree)
    {
        tree.GetOrAddTreeAttribute(attributeBanner).RemoveAttribute(attributePlacement);
    }

    public static ITreeAttribute GetBannerTree(ITreeAttribute tree)
    {
        return tree.GetOrAddTreeAttribute(attributeBanner);
    }

    public override string ToString()
    {
        return Layers.Any()
            ? $"{Name}-{Placement}-{string.Join(layerSeparator, Layers.Select(x => $"{x.Key}-{x.Value}"))}"
            : $"{Name}-{Placement}";
    }
}