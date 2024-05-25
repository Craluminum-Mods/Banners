namespace Flags;

public class BannerLayer
{
    public string Name => $"{langCodePatternAndColor}".Localize($"{langCodePattern}{Pattern}".Localize(), $"{langCodeColor}{Color}".Localize());
    public string Priority { get; protected set; }
    public string Pattern { get; protected set; }
    public string Color { get; protected set; }
    public string OldTextureCode { get; protected set; }

    public string TextureCode => $"{OldTextureCode}-{Pattern}";

    public BannerLayer(string layer, string color, string oldTextureCode = null)
    {
        string[] values = layer.Split("-");
        if (values.Length != bannerCodeMaxElements)
        {
            return;
        }

        Priority = values[0];
        Pattern = values[1];
        Color = color;

        if (!string.IsNullOrEmpty(oldTextureCode))
        {
            OldTextureCode = oldTextureCode;
        }
    }

    public BannerLayer(string pattern, BannerLiquid liquid)
    {
        Pattern = pattern;
        Color = liquid.Color;
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