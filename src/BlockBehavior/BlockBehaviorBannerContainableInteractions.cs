using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Flags;

public class BlockBehaviorBannerContainableInteractions : BlockBehavior
{
    public BlockBehaviorBannerContainableInteractions(Block block) : base(block) { }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
    {
        if (world.Api is not ICoreClientAPI capi)
        {
            return Array.Empty<WorldInteraction>();
        }

        handling = EnumHandling.Handled;
        return ContainableInteractions(capi);
    }

    public WorldInteraction[] ContainableInteractions(ICoreClientAPI capi)
    {
        List<ItemStack> bannerStacks = new();
        foreach (ItemStack stack in capi.World.Collectibles.Where(obj => obj is BlockBanner).SelectMany(obj => obj.GetHandBookStacks(capi) ?? Array.Empty<ItemStack>().ToList()))
        {
            ItemStack newStack = stack.Clone();
            newStack.StackSize = 1;
            bannerStacks.Add(newStack);
        }

        return new WorldInteraction[] {
            new WorldInteraction()
            {
                ActionLangCode = langCodeBannerContainableContainedBannerAdd,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = bannerStacks.ToArray()
            },
            new WorldInteraction()
            {
                ActionLangCode = langCodeBannerContainableContainedBannerRemove,
                MouseButton = EnumMouseButton.Left
            },
        };
    }
}