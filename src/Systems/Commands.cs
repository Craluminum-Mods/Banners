using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Util;

namespace Flags;

public class Commands : ModSystem
{
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI capi)
    {
        IChatCommand commands = capi.ChatCommands.GetOrCreate("flags");
        CommandArgumentParsers parsers = capi.ChatCommands.Parsers;

        commands.BeginSub("gentextures")
            .WithAlias("gentex")
            .WithDesc(Constants.Commands.GenerateTextures.Description.Localize())
            .WithArgs(
            parsers.Bool(Constants.Commands.GenerateTextures.OptionalArgReplaceExisting.Localize()),
            parsers.Word(Constants.Commands.GenerateTextures.OptionalArgGrayscaleColor.Localize()))
            .HandleWith(GenerateTextures)
        .EndSub();
    }

    public static TextCommandResult GenerateTextures(TextCommandCallingArgs args)
    {
        bool replaceExisting = args?[0].ToString().ToBool() ?? false;
        string grayscaleColor = args.LastArg.ToString();
        ItemSlot slot = args.Caller.Player.Entity.RightHandItemSlot;
        ICoreClientAPI capi = args.Caller.Player.Entity.Api as ICoreClientAPI;
        if (slot?.Itemstack?.Collectible is not BlockBanner blockBanner)
        {
            return TextCommandResult.Error(Constants.Commands.GenerateTextures.ErrorNoBanner.Localize());
        }
        blockBanner.DebugPregenerateTextures(capi, replaceExisting, grayscaleColor);
        return TextCommandResult.Success(Constants.Commands.GenerateTextures.Success.Localize());
    }
}