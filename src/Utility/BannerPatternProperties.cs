using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Flags;

public class BannerPatternProperties
{
    public string Type { get; protected set; }

    public BannerPatternProperties(string defaultType)
    {
        Type = !string.IsNullOrEmpty(defaultType) ? defaultType : Type;
    }

    public string GetTextureCode(string oldTextureCode)
    {
        return $"{oldTextureCode}-{Type}";
    }

    public BannerPatternProperties FromTreeAttribute(ITreeAttribute tree)
    {
        Type = tree.GetOrAddTreeAttribute(attributeBannerPattern).GetString(attributeType);
        return this;
    }

    public void ToTreeAttribute(ITreeAttribute tree)
    {
        tree.GetOrAddTreeAttribute(attributeBannerPattern).SetString(attributeType, Type);
    }

    public static BannerPatternProperties FromStack(ItemStack stack, ItemBannerPattern item = null)
    {
        string defaultType = item?.DefaultType ?? (stack.Collectible as ItemBannerPattern)?.DefaultType;
        return new BannerPatternProperties(defaultType).FromTreeAttribute(stack.Attributes);
    }

    public void SetType(string type)
    {
        Type = type;
    }

    public override string ToString()
    {
        return Type;
    }
}