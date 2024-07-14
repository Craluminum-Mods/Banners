using Vintagestory.API.Config;

namespace Flags;

public static class StringExtensions
{
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
}