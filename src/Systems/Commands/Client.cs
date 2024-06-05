using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Util;

namespace Flags.Commands;

public class Client : ModSystem
{
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI capi)
    {
        IChatCommand commands = capi.ChatCommands.GetOrCreate("flags").WithRootAlias("banners");
        CommandArgumentParsers parsers = capi.ChatCommands.Parsers;

        commands.BeginSub("gentextures")
            .WithAlias("gentex")
            .WithDesc($"{modDomain}:command-generatetextures-desc".Localize(OutputFolder))
            .WithArgs(
            parsers.OptionalBool("replace"))
            .HandleWith(GenerateTextures)
        .EndSub();
    }

    public static TextCommandResult GenerateTextures(TextCommandCallingArgs args)
    {
        bool replaceExisting = args?[0].ToString().ToBool() ?? false;
        ItemSlot slot = args.Caller.Player.Entity.RightHandItemSlot;
        ICoreClientAPI capi = args.Caller.Player.Entity.Api as ICoreClientAPI;
        return slot?.Itemstack?.Collectible is BlockBanner blockBanner
            ? blockBanner.DebugPregenerateTextures(capi, replaceExisting)
            : TextCommandResult.Error($"{modDomain}:command-nobanner".Localize());
    }
}