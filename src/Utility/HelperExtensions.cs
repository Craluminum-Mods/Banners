using Vintagestory.API.Common;
using Vintagestory.API.Config;

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
            return input.Substring(0, lastIndexOfSymbol);
        }
        else
        {
            return input;
        }
    }

    public static string Localize(this string input, params string[] args)
    {
        return Lang.Get(input, args);
    }

    public static string LocalizeM(this string input, params string[] args)
    {
        return Lang.GetMatching(input, args);
    }
}