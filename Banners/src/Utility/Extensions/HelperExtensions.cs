using Cairo;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Flags;

public static class HelperExtensions
{
    public static bool IsProtected(this BlockSelection selection, IWorldAccessor world, IPlayer forPlayer, EnumBlockAccessFlags accessFlags)
    {
        bool _protected = false;
        if (world.Claims != null && forPlayer?.WorldData.CurrentGameMode == EnumGameMode.Survival && world.Claims.TestAccess(forPlayer, selection.Position, accessFlags) != 0)
        {
            _protected = true;
        }
        return _protected;
    }

    public static string RemoveAfterLastSymbol(this string input, char symbol)
    {
        int lastIndexOfSymbol = input.LastIndexOf(symbol);
        if (lastIndexOfSymbol >= 0)
        {
            return input[..lastIndexOfSymbol];
        }
        else
        {
            return input;
        }
    }

    public static string Localize(this string input, params object[] args)
    {
        return Lang.Get(input, args);
    }

    public static string LocalizeM(this string input, params object[] args)
    {
        return Lang.GetMatching(input, args);
    }

    public static bool HasTranslation(this string key) => Lang.HasTranslation(key);


    public static double Scaled(this double value)
    {
        return GuiElement.scaled(value);
    }

    public static double Scaledi(this double value)
    {
        return GuiElement.scaledi(value);
    }

    public static bool TryGetBEBehavior<T>(this IBlockAccessor blockAccessor, BlockSelection blockSel, out T behavior) where T : BlockEntityBehavior
    {
        if (blockSel == null)
        {
            behavior = null;
            return false;
        }
        behavior = blockAccessor.GetBlockEntity(blockSel.Position)?.GetBehavior<T>();
        return behavior != null;
    }

    public static bool TryGetBEBehavior<T>(this IBlockAccessor blockAccessor, BlockPos pos, out T behavior) where T : BlockEntityBehavior
    {
        behavior = blockAccessor.GetBlockEntity(pos)?.GetBehavior<T>();
        return behavior != null;
    }

    public static bool TryGetBlockBehavior<T>(this IBlockAccessor blockAccessor, BlockSelection blockSel, out T behavior) where T : BlockBehavior
    {
        if (blockSel == null)
        {
            behavior = null;
            return false;
        }
        behavior = blockAccessor.GetBlock(blockSel.Position)?.GetBehavior<T>();
        return behavior != null;
    }

    public static ItemStack[] GetHandBookStacksArray(this CollectibleObject obj, ICoreClientAPI capi)
    {
        return obj.GetHandBookStacks(capi)?.ToArray() ?? System.Array.Empty<ItemStack>();
    }

    public static RenderSkillItemDelegate RenderItemStack(this ItemStack stack, ICoreClientAPI capi, bool showStackSize = false)
    {
        return (AssetLocation code, float dt, double posX, double posY) =>
        {
            double size = GuiElementPassiveItemSlot.unscaledSlotSize + GuiElementItemSlotGridBase.unscaledSlotPadding;
            double scsize = GuiElement.scaled(size - 5);

            capi.Render.RenderItemstackToGui(
                new DummySlot(stack),
                posX + (scsize / 2),
                posY + (scsize / 2),
                100,
                (float)GuiElement.scaled(GuiElementPassiveItemSlot.unscaledItemSize),
                ColorUtil.WhiteArgb,
                showStackSize: showStackSize);
        };
    }

    public static bool TryGetValueOrWildcard<T>(this Dictionary<string, T> dict, string key, out T value)
    {
        return dict.TryGetValue(key, out value) || dict.TryGetValue(Wildcard, out value);
    }

    public static bool IsCreative(this IPlayer byPlayer)
    {
        return byPlayer != null && byPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative;
    }

    public static void IngameError(this IPlayer byPlayer, object sender, string errorCode, string text)
    {
        (byPlayer.Entity.World.Api as ICoreClientAPI)?.TriggerIngameError(sender, errorCode, text);
    }

    public static bool MatchesPatternGroups(this BlockBanner block, ItemBannerPattern item)
    {
        return block.PatternGroups.Any(item.PatternGroups.Contains);
    }

    public static bool MatchesPatternGroups(this BlockBanner block, BlockBanner otherBlock)
    {
        return block.PatternGroups.Any(otherBlock.PatternGroups.Contains);
    }

    public static bool IsEditModeEnabled(this BannerProperties props, IPlayer byPlayer, bool printError = true)
    {
        BannerModes modes = props.Modes;
        if (!modes[BannerMode.EditMode_Off])
        {
            return modes[BannerMode.EditMode_On];
        }
        if (printError)
        {
            byPlayer?.IngameError(props, modes.ErrorCode(BannerMode.EditMode_Off.Key), modes.ErrorCode(BannerMode.EditMode_Off.Key).Localize());
        }
        return false;
    }

    public static bool IsEditModeEnabled(this BannerProperties props, ICoreClientAPI capi, bool printError = true)
    {
        return props.IsEditModeEnabled(capi.World.Player, printError);
    }
}