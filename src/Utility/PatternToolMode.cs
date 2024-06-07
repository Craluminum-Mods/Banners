using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Flags;

public class PatternToolMode
{
    public string Pattern { get; set; }
    public bool Linebreak { get; set; }
    public Unlockable[] Unlockable { get; set; } = null;

    public SkillItem GetToolMode(ICoreClientAPI capi, ItemSlot slot)
    {
        ItemStack newStack = slot.Itemstack.Clone();
        SetPattern(newStack);
        bool unlocked = IsUnlocked(slot) || capi.World.Player.WorldData.CurrentGameMode is EnumGameMode.Creative;

        SkillItem toolMode = new SkillItem()
        {
            Linebreak = Linebreak,
            Name = GetName(unlocked)
        };

        if (unlocked)
        {
            toolMode.RenderHandler = newStack.RenderItemStack(capi, showStackSize: false);
        }
        else
        {
            toolMode.Texture = ObjectCacheUtil.TryGet<LoadedTexture>(capi, cacheKeyQuestionTexture);
        }

        return toolMode;
    }

    private string GetName(bool unlocked)
    {
        if (unlocked)
        {
            return $"{langCodePattern}{Pattern}".Localize();
        }
        else
        {
            return langCodePatternLocked.Localize();
        }
    }

    public static SkillItem[] GetToolModes(ICoreClientAPI capi, ItemSlot slot, IEnumerable<PatternToolMode> toolModes)
    {
        return toolModes?.Select(x => x.GetToolMode(capi, slot)).ToArray();
    }

    public void SetPattern(ItemStack stack)
    {
        BannerPatternProperties props = BannerPatternProperties.FromStack(stack);
        props.SetType(Pattern);
        props.ToTreeAttribute(stack.Attributes);
    }

    public bool TryUnlock(ItemSlot slot, ItemStack byStack, bool skipStack = false)
    {
        BannerPatternProperties props = BannerPatternProperties.FromStack(slot.Itemstack);
        if ((skipStack && props.Type == Pattern) || Unlockable.Any(x => x.Matches(byStack)))
        {
            props.SetUnlockedTypes(Pattern);
            props.ToTreeAttribute(slot.Itemstack.Attributes);
            slot.MarkDirty();
            return true;
        }
        else if (byStack?.Collectible is ItemBannerPattern)
        {
            BannerPatternProperties otherProps = BannerPatternProperties.FromStack(byStack);
            props.MergeTypes(otherProps);
            props.ToTreeAttribute(slot.Itemstack.Attributes);
            slot.MarkDirty();
            return true;
        }
        return false;
    }

    public static bool TryUnlockAll(IEnumerable<PatternToolMode> toolModes, ItemSlot slot, ItemStack byStack, bool skipStack = false)
    {
        bool any = false;
        foreach (PatternToolMode toolMode in toolModes.Where(toolMode => !toolMode.IsUnlocked(slot)))
        {
            if (toolMode.TryUnlock(slot, byStack, skipStack))
            {
                any = true;
            }
        }
        return any;
    }

    public bool IsUnlocked(ItemSlot slot)
    {
        return Unlockable == null || (Unlockable != null && BannerPatternProperties.FromStack(slot.Itemstack).IsUnlocked(Pattern));
    }
}

public class Unlockable
{
    public string Code { get; set; }
    public EnumItemClass Type { get; set; }
    public Dictionary<string, string> HasAttributes { get; set; } = new();

    public bool Matches(ItemStack stack)
    {
        return stack != null && !string.IsNullOrEmpty(Code)
            && new AssetLocation(Code) == stack.Collectible.Code
            && Type == stack.Class
            && (HasAttributes == null || (HasAttributes != null && HasAttributes.All(x => stack.Attributes.GetAsString(x.Key) == x.Value)));
    }
}