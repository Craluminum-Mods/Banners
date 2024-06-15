using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Flags;

public class PatternProperties
{
    public string Type { get; protected set; } = string.Empty;
    public List<string> UnlockedTypes { get; protected set; } = new();

    public string UnlockedTypesAsString => string.Join(unlockedSeparator, UnlockedTypes).TrimStart(unlockedSeparator) ?? "";

    public string GetTextureCode(string oldTextureCode)
    {
        return $"{oldTextureCode}-{Type}";
    }

    public PatternProperties FromTreeAttribute(ITreeAttribute tree)
    {
        Type = tree.GetOrAddTreeAttribute(attributeBannerPattern).GetString(attributeType);
        UnlockedTypes = tree.GetOrAddTreeAttribute(attributeBannerPattern).GetAsString(attributeUnlockedTypes, string.Empty).Split(unlockedSeparator).ToList();
        return this;
    }

    public void ToTreeAttribute(ITreeAttribute tree)
    {
        tree.GetOrAddTreeAttribute(attributeBannerPattern).SetString(attributeType, Type ?? "");
        tree.GetOrAddTreeAttribute(attributeBannerPattern).SetString(attributeUnlockedTypes, UnlockedTypesAsString);
    }

    public static PatternProperties FromStack(ItemStack stack)
    {
        return new PatternProperties().FromTreeAttribute(stack.Attributes);
    }

    public void SetType(string type)
    {
        Type = type;
    }

    public void SetUnlockedTypes(params string[] types)
    {
        UnlockedTypes.AddRange(types.Where(type => !UnlockedTypes.Contains(type)));
        UnlockedTypes = UnlockedTypes.Distinct().Order().ToList();
    }

    public void MergeTypes(PatternProperties otherProps)
    {
        if (!string.IsNullOrEmpty(otherProps.Type))
        {
            SetUnlockedTypes(otherProps.Type);
        }
        if (otherProps.UnlockedTypes != null && otherProps.UnlockedTypes.Any())
        {
            SetUnlockedTypes(otherProps.UnlockedTypes.ToArray());
        }
    }

    public bool IsUnlocked(string type)
    {
        return UnlockedTypes != null && UnlockedTypes.Contains(type);
    }

    public override string ToString()
    {
        return $"{Type}-{UnlockedTypesAsString}";
    }
}