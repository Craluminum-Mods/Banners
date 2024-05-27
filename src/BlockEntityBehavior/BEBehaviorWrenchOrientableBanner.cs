using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Flags;

public class BEBehaviorWrenchOrientableBanner : BlockEntityBehavior, IWrenchOrientable
{
    public Dictionary<string, List<string>> Placements { get; protected set; }
    public Dictionary<string, string> PlacementBaseCodes { get; protected set; }

    public BEBehaviorWrenchOrientableBanner(BlockEntity blockentity) : base(blockentity) { }

    public override void Initialize(ICoreAPI api, JsonObject properties)
    {
        base.Initialize(api, properties);

        Placements = properties[attributePlacements].AsObject<Dictionary<string, List<string>>>();
        PlacementBaseCodes = properties[attributePlacementBaseCodes].AsObject<Dictionary<string, string>>();
    }

    public void Rotate(EntityAgent byEntity, BlockSelection blockSel, int dir)
    {
        if (Api.World.BlockAccessor.GetBlockEntity(Pos) is not BlockEntityBanner be)
        {
            return;
        }

        IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
        if (byPlayer == null || !byEntity.World.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
        {
            return;
        }

        bool sneak = byEntity.Controls.Sneak;
        bool sprint = byEntity.Controls.Sprint;

        BannerProperties BannerProps = be.BannerProps;
        IRotatableBanner rotatableBanner = Block.GetInterface<IRotatableBanner>(Api.World, Pos);
        if (rotatableBanner?.TryRotate(byEntity, blockSel, dir) == true)
        {

        }
        else if (sneak && sprint
        && (PlacementBaseCodes?.TryGetValue(BannerProps.Placement, out string baseCode) == true || PlacementBaseCodes?.TryGetValue(Wildcard, out baseCode) == true)
        && (Placements?.TryGetValue(baseCode, out List<string> possibleTypes) == true || Placements?.TryGetValue(Wildcard, out possibleTypes) == true))
        {
            using List<string>.Enumerator types = possibleTypes.GetEnumerator();
            while (types.MoveNext())
            {
                if (types.Current != null && types.Current.Equals(BannerProps.Placement))
                {
                    break;
                }
            }

            string newType = types.MoveNext() ? types.Current : possibleTypes.First();
            BannerProps.SetPlacement(newType);

            Api.World.PlaySoundAt(Block.Sounds.Place, blockSel.Position.X + 0.5f, blockSel.Position.Y + 0.5f, blockSel.Position.Z + 0.5f, byPlayer);
            (Api.World as IClientWorldAccessor)?.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
        }

        be.GetOrCreateCollisionBoxes(true);
        be.GetOrCreateSelectionBoxes(true);
        be.MarkDirty(redrawOnClient: true);
    }

    public WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, List<ItemStack> wrenchStacks)
    {
        if (selection.IsProtected(world, forPlayer, EnumBlockAccessFlags.BuildOrBreak))
        {
            return Array.Empty<WorldInteraction>();
        }

        return new WorldInteraction[]
        {
            new()
            {
                ActionLangCode = langCodeCycleVariants,
                MouseButton = EnumMouseButton.Left,
                HotKeyCodes = new[] { "shift", "ctrl" },
                Itemstacks = wrenchStacks?.ToArray()
            }
        };
    }
}