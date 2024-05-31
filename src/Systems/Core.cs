global using static Flags.Constants;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

[assembly: ModInfo(name: "Banners", modID: "flags")]

namespace Flags;

public class Core : ModSystem
{
    public static BannerConverter Converter { get; set; } = new();

    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("Flags.BlockBanner", typeof(BlockBanner));
        api.RegisterBlockBehaviorClass("Flags.BannerName", typeof(BlockBehaviorBannerName));
        api.RegisterBlockBehaviorClass("Flags.BannerInteractions", typeof(BlockBehaviorBannerInteractions));
        api.RegisterBlockBehaviorClass("Flags.BannerContainableInteractions", typeof(BlockBehaviorBannerContainableInteractions));
        api.RegisterBlockEntityClass("Flags.Banner", typeof(BlockEntityBanner));
        api.RegisterBlockEntityBehaviorClass("Flags.Banner.Rotatable", typeof(BEBehaviorRotatableBanner));
        api.RegisterBlockEntityBehaviorClass("Flags.Banner.WrenchOrientable", typeof(BEBehaviorWrenchOrientableBanner));
        api.RegisterBlockEntityBehaviorClass("Flags.BannerContainable", typeof(BEBehaviorBannerContainable));
        api.RegisterItemClass("Flags.ItemRollableFixed", typeof(ItemRollableFixed));
        api.RegisterItemClass("Flags.ItemBannerPattern", typeof(ItemBannerPattern));
        api.RegisterCollectibleBehaviorClass("Flags.BannerPatternName", typeof(CollectibleBehaviorBannerPatternName));
        api.RegisterCollectibleBehaviorClass("Flags.BannerLiquidDescription", typeof(CollectibleBehaviorBannerLiquidDescription));
        api.Logger.Event("started '{0}' mod", Mod.Info.Name);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        if (!api.World.Config.HasAttribute(worldConfigLayersLimit))
        {
            api.World.Config.SetInt(worldConfigLayersLimit, defaultLayersLimit);
        }
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        foreach (CollectibleObject obj in api.World.Collectibles)
        {
            if (BannerLiquid.HasAttribute(obj))
            {
                if (!obj.HasBehavior<CollectibleBehaviorBannerLiquidDescription>())
                {
                    obj.CollectibleBehaviors = obj.CollectibleBehaviors.Append(new CollectibleBehaviorBannerLiquidDescription(obj));
                }

                foreach (CreativeTabAndStackList item in obj.CreativeInventoryStacks)
                {
                    item.Tabs = item?.Tabs?.Append(modCreativeTab);
                }
            }
            if (obj is BlockLiquidContainerTopOpened && !obj.HasBehavior<CollectibleBehaviorBannerLiquidDescription>())
            {
                obj.CollectibleBehaviors = obj.CollectibleBehaviors.Append(new CollectibleBehaviorBannerLiquidDescription(obj));
            }
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        Converter = api.Assets.TryGet(AssetLocation.Create(pathConverter)).ToObject<BannerConverter>();
    }
}