using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Flags;

public class CachedInteractions : ModSystem
{
    private ICoreClientAPI capi;

    public override bool ShouldLoad(EnumAppSide forSide) => forSide.IsClient();

    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        ObjectCacheUtil.GetOrCreate(capi, cacheKeyBannerStacks, () =>
        {
            return capi.World.Collectibles.Where(obj => obj is BlockBanner).SelectMany(obj => obj.GetHandBookStacksArray(capi)).Select(stack =>
            {
                ItemStack newStack = stack.Clone();
                newStack.StackSize = 1;
                return newStack;
            }).ToArray();
        });

        ObjectCacheUtil.GetOrCreate(capi, cacheKeyBookStacks, () =>
        {
            return capi.World.Collectibles.Where(obj => obj is ItemBook).SelectMany(obj => obj.GetHandBookStacksArray(capi)).ToArray();
        });

        ObjectCacheUtil.GetOrCreate(capi, cacheKeyWrenchStacks, () =>
        {
            return capi.World.Collectibles.Where(obj => obj is ItemWrench).SelectMany(obj => obj.GetHandBookStacksArray(capi)).ToArray();
        });

        ObjectCacheUtil.GetOrCreate(capi, cacheKeyDyeStacks, () =>
        {
            return capi.World.Collectibles.Where(obj => BannerLiquid.TryGet(obj, out BannerLiquid liquid) && liquid.IsDye).SelectMany(obj => obj.GetHandBookStacksArray(capi)).ToArray();
        });

        ObjectCacheUtil.GetOrCreate(capi, cacheKeyBleachStacks, () =>
        {
            return capi.World.Collectibles.Where(obj => BannerLiquid.TryGet(obj, out BannerLiquid liquid) && liquid.IsBleach).SelectMany(obj => obj.GetHandBookStacksArray(capi)).ToArray();
        });

        ObjectCacheUtil.GetOrCreate(capi, cacheKeyRotatableBannerInteractions, () =>
        {
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = langCodeRotateBy22_5,
                    MouseButton = EnumMouseButton.Left,
                    Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyWrenchStacks)
                },
                new WorldInteraction()
                {
                    ActionLangCode = langCodeRotateByAxisBy90,
                    MouseButton = EnumMouseButton.Left,
                    HotKeyCodes = new[] { "shift" },
                    Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyWrenchStacks)
                },
                new WorldInteraction()
                {
                    ActionLangCode = langCodeClearRotationsXZ,
                    MouseButton = EnumMouseButton.Left,
                    HotKeyCodes = new[] { "ctrl" },
                    Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyWrenchStacks)
                },
            };
        });

        ObjectCacheUtil.GetOrCreate(capi, cacheKeyWrenchableBannerInteractions, () =>
        {
            return new WorldInteraction()
            {
                ActionLangCode = langCodeSwapModel,
                MouseButton = EnumMouseButton.Left,
                HotKeyCodes = new[] { "shift", "ctrl" },
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(capi, cacheKeyWrenchStacks)
            };
        });
    }

    public override void Dispose()
    {
        ObjectCacheUtil.Delete(capi, cacheKeyBannerStacks);
        ObjectCacheUtil.Delete(capi, cacheKeyBookStacks);
        ObjectCacheUtil.Delete(capi, cacheKeyWrenchStacks);
        ObjectCacheUtil.Delete(capi, cacheKeyDyeStacks);
        ObjectCacheUtil.Delete(capi, cacheKeyBleachStacks);
        ObjectCacheUtil.Delete(capi, cacheKeyRotatableBannerInteractions);
        ObjectCacheUtil.Delete(capi, cacheKeyWrenchableBannerInteractions);
    }
}