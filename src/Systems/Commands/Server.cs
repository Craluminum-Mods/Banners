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

        // commands.BeginSub("rotatecontainable")
        //     .WithAlias("rotcnt")
        //     // .WithDesc($"{modDomain}:command-genbannermc-desc".Localize())
        //     .WithArgs(
        //         parsers.OptionalBool("toggle debug"),
        //         parsers.OptionalWord("axis"),
        //         parsers.OptionalFloat("rotation")
        //     )
        //     .HandleWith(delegate (TextCommandCallingArgs args)
        //     {
        //         BlockSelection blockSel = args.Caller.Player.CurrentBlockSelection;
        //         BlockEntity blockEntity = args.Caller.Player.Entity.World.BlockAccessor.GetBlockEntity(blockSel?.Position);
        //         return args.Caller.Player.Entity.World.BlockAccessor.TryGetBEBehavior(blockSel, out BEBehaviorBannerContainable bebehavior)
        //             ? bebehavior.DebugRotate(blockSel, (bool)args?[0], (string)args?[1], (float)args?[2])
        //             : TextCommandResult.Deferred;
        //     })
        // .EndSub();
    }
}