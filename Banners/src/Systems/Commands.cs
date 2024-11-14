using Flags;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;

namespace Flags;

public class Commands : ModSystem
{
    public override void StartServerSide(ICoreServerAPI sapi)
    {
        BannerConverter converterSystem = sapi.ModLoader.GetModSystem<BannerConverter>();

        IChatCommand commands = sapi.ChatCommands.GetOrCreate("flags")
            .WithRootAlias("banners")
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat);

        CommandArgumentParsers parsers = sapi.ChatCommands.Parsers;

        commands.BeginSub("genbannermc")
            .WithAlias("genmc")
            .WithDesc($"{modDomain}:command-genbannermc-desc".Localize())
            .RequiresPrivilege(Privilege.give)
            .WithArgs(
            parsers.Unparsed("minecraft syntax"))
            .HandleWith(BannerConverter.TryGenerateBanner)
        .EndSub();
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        IChatCommand commands = capi.ChatCommands.GetOrCreate("flags").WithRootAlias("banners");
        CommandArgumentParsers parsers = capi.ChatCommands.Parsers;

        commands.BeginSub("gentextures")
            .WithAlias("gentex")
            .WithDesc($"{modDomain}:command-generatetextures-desc".Localize(OutputFolder))
            .WithArgs(
            parsers.OptionalBool("replace"))
            .HandleWith((args) => args.GenerateTextures())
        .EndSub();
    }
}