using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Flags;

public static class LiquidExtensions
{
    public static void DoLiquidMovedEffects(this IPlayer player, ItemStack contentStack, int moved, BlockLiquidContainerBase.EnumLiquidDirection dir)
    {
        if (player == null || contentStack == null)
        {
            return;
        }
        WaterTightContainableProps props = BlockLiquidContainerBase.GetContainableProps(contentStack);
        if (props == null)
        {
            return;
        }
        float litresMoved = (float)moved / props.ItemsPerLitre;
        (player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
        player.Entity.World.PlaySoundAt((dir == BlockLiquidContainerBase.EnumLiquidDirection.Fill) ? props.FillSound : props.PourSound, player.Entity, player, randomizePitch: true, 16f, GameMath.Clamp(litresMoved / 5f, 0.35f, 1f));
        player.Entity.World.SpawnCubeParticles(player.CurrentBlockSelection.FullPosition, contentStack, 0.75f, (int)litresMoved * 2, 0.45f);
    }
}