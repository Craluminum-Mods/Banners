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
            .HandleWith(Core.Converter.TryGenerateBanner)
        .EndSub();

    }
}