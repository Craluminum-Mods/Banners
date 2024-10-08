global using static Flags.Constants;
using Flags.Converter;
using Newtonsoft.Json.Linq;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

[assembly: ModInfo(name: "Banners", modID: modDomain)]

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
        api.RegisterBlockBehaviorClass("Flags.BannerToolModes", typeof(BlockBehaviorBannerToolModes));
        api.RegisterBlockEntityClass("Flags.Banner", typeof(BlockEntityBanner));
        api.RegisterBlockEntityBehaviorClass("Flags.Banner.Rotatable", typeof(BEBehaviorRotatableBanner));
        api.RegisterBlockEntityBehaviorClass("Flags.Banner.WrenchOrientable", typeof(BEBehaviorWrenchOrientableBanner));
        api.RegisterBlockEntityBehaviorClass("Flags.BannerContainable", typeof(BEBehaviorBannerContainable));
        api.RegisterItemClass("Flags.ItemRollableFixed", typeof(ItemRollableFixed));
        api.RegisterItemClass("Flags.ItemBannerPattern", typeof(ItemBannerPattern));
        api.RegisterCollectibleBehaviorClass("Flags.BannerPatternName", typeof(CollectibleBehaviorBannerPatternName));
        api.RegisterCollectibleBehaviorClass("Flags.BannerPatternDescription", typeof(CollectibleBehaviorBannerPatternDescription));
        api.RegisterCollectibleBehaviorClass("Flags.BannerPatternToolModes", typeof(CollectibleBehaviorBannerPatternToolModes));
        api.RegisterCollectibleBehaviorClass("Flags.BannerLiquidDescription", typeof(CollectibleBehaviorBannerLiquidDescription));
        api.RegisterCollectibleBehaviorClass("Flags.CutoutTool", typeof(CollectibleBehaviorCutoutTool));
        api.RegisterCollectibleBehaviorClass("Flags.RenameTool", typeof(CollectibleBehaviorRenameTool));
        api.RegisterEntityBehaviorClass("flags:bannercontainableboat", typeof(EntityBehaviorBoatWithBanner));
        api.Logger.Event("started '{0}' mod", Mod.Info.Name);

        GlobalConstants.IgnoredStackAttributes = GlobalConstants.IgnoredStackAttributes.Append(BannersIgnoreAttributeSubTrees);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        if (!api.World.Config.HasAttribute(worldConfigLayersLimit))
        {
            api.World.Config.SetInt(worldConfigLayersLimit, defaultLayersLimit);
        }

        api.Event.OnEntitySpawn += AddEntityBehaviors;
        api.Event.OnEntityLoaded += AddEntityBehaviors;
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Gui.RegisterDialog(new HudElementBannerPreview(api), new HudElementBannerOverview(api), new GuiDialogBannerConfig(api));
        GuiDialogTransformEditor.extraTransforms.Add(new TransformConfig() { Title = langCodeBannerPreviewHudTransform.Localize(), AttributeName = attributeBannerPreviewHudTransform });
        GuiDialogTransformEditor.extraTransforms.Add(new TransformConfig() { Title = langCodeBannerOnBoatTransform.Localize(), AttributeName = attributeBannerOnBoatTransform });

        api.Event.OnEntitySpawn += AddEntityBehaviors;
        api.Event.OnEntityLoaded += AddEntityBehaviors;
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
            if (obj is ItemShears and not ItemScythe && !obj.HasBehavior<CollectibleBehaviorCutoutTool>())
            {
                obj.CollectibleBehaviors = obj.CollectibleBehaviors.Append(new CollectibleBehaviorCutoutTool(obj));
            }
            if (obj is ItemBook && !obj.HasBehavior<CollectibleBehaviorRenameTool>())
            {
                obj.CollectibleBehaviors = obj.CollectibleBehaviors.Append(new CollectibleBehaviorRenameTool(obj));

                if (obj.Code == AssetLocation.Create(itemcodeParchment))
                {
                    obj.CreativeInventoryTabs = obj.CreativeInventoryTabs.Append(modCreativeTab);
                }
            }
            if (obj.Code == AssetLocation.Create(itemcodeInkAndQuill))
            {
                obj.CreativeInventoryTabs = obj.CreativeInventoryTabs.Append(modCreativeTab);
            }
        }

        CollectibleObject cutoutTool = api.World.Collectibles
            .Where(obj => obj != null && obj.Code != null && obj.HasBehavior<CollectibleBehaviorCutoutTool>() && obj.CreativeInventoryTabs != null)
            ?.OrderByDescending(obj => obj.Durability)
            ?.FirstOrDefault();

        if (cutoutTool != null)
        {
            cutoutTool.CreativeInventoryTabs = cutoutTool.CreativeInventoryTabs.Append(modCreativeTab);
        }

        CollectibleObject wrenchTool = api.World.Collectibles
            .Where(obj => obj is ItemWrench && obj.CreativeInventoryTabs != null)
            ?.OrderByDescending(obj => obj.Durability)
            ?.FirstOrDefault();

        if (wrenchTool != null)
        {
            wrenchTool.CreativeInventoryTabs = wrenchTool.CreativeInventoryTabs.Append(modCreativeTab);
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        Converter = api.Assets.TryGet(AssetLocation.Create(pathConverter)).ToObject<BannerConverter>();
    }

    private void AddEntityBehaviors(Entity entity)
    {
        if (entity.HasBehavior<EntityBehaviorBoatWithBanner>())
        {
            return;
        }
        if ((entity.Api.ModLoader.IsModEnabled("sailboat") && entity.Code.Domain == "sailboat") || entity is EntityBoat)
        {
            EntityBehaviorBoatWithBanner behavior = new EntityBehaviorBoatWithBanner(entity);
            behavior.Initialize(entity.Properties, new JsonObject(new JObject()));
            entity.AddBehavior(behavior);
        }
    }
}