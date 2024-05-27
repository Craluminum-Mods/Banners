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
            .WithDesc(Constants.Commands.GenerateBannerMC.Description.Localize())
            .WithArgs(
            parsers.Unparsed(Constants.Commands.GenerateBannerMC.ArgCommandMC.Localize()))
            .HandleWith(GenerateBannerMC)
        .EndSub();
    }

    public static TextCommandResult GenerateBannerMC(TextCommandCallingArgs args)
    {
        if (args.Caller.Player.Entity.RightHandItemSlot?.Itemstack?.Collectible is not BlockBanner)
        {
            return TextCommandResult.Error(Constants.Commands.ErrorNoBanner.Localize());
        }

        return Core.Converter.TryGenerateBanner(args)
            ? TextCommandResult.Success(Constants.Commands.GenerateBannerMC.Success.Localize())
            : TextCommandResult.Error(Constants.Commands.GenerateBannerMC.ErrorSyntax.Localize());
    }
}
