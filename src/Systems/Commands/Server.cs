using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;

namespace Flags.Commands;

public class Server : ModSystem
{
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    public override void StartServerSide(ICoreServerAPI sapi)
    {
        IChatCommand commands = sapi.ChatCommands.GetOrCreate("flags").WithRootAlias("banners").RequiresPlayer().RequiresPrivilege(Privilege.give);
        CommandArgumentParsers parsers = sapi.ChatCommands.Parsers;

        commands.BeginSub("genbannermc")
            .WithAlias("genmc")
            .WithDesc($"{modDomain}:command-genbannermc-desc".Localize())
            .WithArgs(
            parsers.Unparsed("minecraft syntax"))
            .HandleWith(GenerateBannerMC)
        .EndSub();
    }

    public static TextCommandResult GenerateBannerMC(TextCommandCallingArgs args)
    {
        if (args.Caller.Player.Entity.RightHandItemSlot?.Itemstack?.Collectible is not BlockBanner)
        {
            return TextCommandResult.Error($"{modDomain}:command-nobanner".Localize());
        }

        return Core.Converter.TryGenerateBanner(args)
            ? TextCommandResult.Success($"{modDomain}:command-genbannermc".Localize())
            : TextCommandResult.Error($"{modDomain}:command-genbannermc-invalidsyntax".Localize());
    }
}