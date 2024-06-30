using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Flags;

public class BlockBehaviorWindmillWithBanners : BlockBehavior
{
    public BlockBehaviorWindmillWithBanners(Block block) : base(block) { }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
    {
        if (world.Api is not ICoreClientAPI capi)
        {
            return Array.Empty<WorldInteraction>();
        }

        handling = EnumHandling.Handled;

        return new WorldInteraction[] {
            new WorldInteraction()
            {
                ActionLangCode = langCodeBannerContainableContainedBannerAdd,
                MouseButton = EnumMouseButton.Right,
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyBannerStacks)
            },
            new WorldInteraction()
            {
                ActionLangCode = langCodeBannerContainableContainedBannerRemove,
                MouseButton = EnumMouseButton.Left
            }
        };
    }
}