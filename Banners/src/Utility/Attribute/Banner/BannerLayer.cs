namespace Flags;

public class BannerLayer
{
    public string Priority { get; protected set; }
    public string Pattern { get; protected set; }
    public string Color { get; protected set; }
    public string OldTextureCode { get; protected set; }

    public string LocalizedPattern => $"{langCodePattern}{Pattern}".Localize();
    public string LocalizedColor => $"{langCodeColor}{Color}".Localize();

    public string Name => $"{LocalizedPattern} ({LocalizedColor})";
    public string TextureCode => $"{OldTextureCode}-{Pattern}";

    public static BannerLayer FromLayer(string layer)
    {
        BannerLayer _layer = new();
        string[] values = layer.Split("-");
        if (values.Length != bannerCodeMaxElements) return _layer;
        _layer.WithPriority(values[0]);
        _layer.WithPattern(values[1]);
        return _layer;
    }

    public BannerLayer WithPriority(string priority)
    {
        Priority = priority;
        return this;
    }

    public BannerLayer WithPattern(string pattern)
    {
        Pattern = pattern;
        return this;
    }

    public BannerLayer WithColor(string color)
    {
        Color = color;
        return this;
    }

    public BannerLayer WithTextureCode(string oldTextureCode)
    {
        OldTextureCode = oldTextureCode;
        return this;
    }

    public string AttributeKey(string priority = null)
    {
        return $"{priority ?? Priority}-{Pattern}";
    }

    public string AttributeValue()
    {
        return Color;
    }

    public override string ToString()
    {
        return $"{AttributeKey()}-{AttributeValue()}";
    }
}