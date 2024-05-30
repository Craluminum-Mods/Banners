
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Flags;

public class BannerConverter
{
    public Dictionary<string, string> BaseColorsToColors { get; set; } = new();
    public Dictionary<string, string> IdsToColors { get; set; } = new();
    public Dictionary<string, string> Patterns { get; set; } = new();

    public bool TryGenerateBanner(TextCommandCallingArgs args)
    {
        IPlayer byPlayer = args.Caller.Player;
        ItemSlot slot = byPlayer.Entity.RightHandItemSlot;
        string mcBaseColor = args.RawArgs.PopUntil('{').Replace("minecraft:", "").Replace("_banner", "");
        string bannerAttributes = args.RawArgs.PopAll();

        JsonObject fromObject;
        try
        {
            bannerAttributes = Regex.Replace(bannerAttributes, @"(?<!\\)\""([a-zA-Z0-9_]+)\""(?=\:)", "\"$1\"");
            bannerAttributes = Regex.Replace(bannerAttributes, @"(?<!\\)\b([a-zA-Z_]+)\b(?=\s*[\,\}])", "\"$1\"");
            fromObject = JsonObject.FromJson(bannerAttributes);
        }
        catch (Exception)
        {
            fromObject = null;
        }

        if ((fromObject?.KeyExists("BlockEntityTag")) != true || !fromObject["BlockEntityTag"].KeyExists("Patterns") || !fromObject["BlockEntityTag"]["Patterns"].AsArray().Any())
        {
            return false;
        }

        if (!BaseColorsToColors.TryGetValue(mcBaseColor, out string vsBaseColor) && !BaseColorsToColors.TryGetValue(Wildcard, out vsBaseColor))
        {
            return false;
        }

        BannerProperties bannerProperties = new BannerProperties((slot.Itemstack.Collectible as BlockBanner)?.DefaultPlacement);
        bannerProperties.AddLayer(new BannerLayer("0-b", vsBaseColor), byPlayer.Entity.World);
        foreach (BannerLayer layer in fromObject["BlockEntityTag"]["Patterns"].AsArray().Select((pattern, index) => new BannerLayer(
            layer: $"{index}-{GetPattern(pattern)}",
            color: GetColor(pattern)
            )))
        {
            bannerProperties.AddLayer(layer, byPlayer.Entity.World, byPlayer);
        }

        slot.Itemstack.Attributes.RemoveAttribute(attributeBanner);
        bannerProperties.ToStack(slot.Itemstack);
        slot.MarkDirty();
        return true;
    }

    private string GetPattern(JsonObject pattern)
    {
        _ = Patterns.TryGetValue(pattern["Pattern"].AsString(), out string value) || Patterns.TryGetValue(Wildcard, out value);
        return value;
    }

    private string GetColor(JsonObject pattern)
    {
        _ = IdsToColors.TryGetValue(pattern["Color"].AsInt().ToString(), out string value) || IdsToColors.TryGetValue(Wildcard, out value);
        return value;
    }
}