global using static Flags.Constants;
using Flags.Converter;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;

namespace Flags;

public class Core : ModSystem
{
    public static BannerConverter Converter { get; set; } = new();

    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("Flags.BlockBanner", typeof(BlockBanner));
        api.RegisterBlockEntityClass("Flags.Banner", typeof(BlockEntityBanner));
        api.RegisterItemClass("Flags.ItemBannerPattern", typeof(ItemBannerPattern));

        api.RegisterBlockBehaviorClass("Flags.BannerName", typeof(BlockBehaviorBannerName));
        api.RegisterBlockBehaviorClass("Flags.BannerInteractions", typeof(BlockBehaviorBannerInteractions));
        api.RegisterBlockBehaviorClass("Flags.BannerContainableInteractions", typeof(BlockBehaviorBannerContainableInteractions));
        api.RegisterBlockBehaviorClass("Flags.BannerToolModes", typeof(BlockBehaviorBannerToolModes));

        api.RegisterBlockEntityBehaviorClass("Flags.Banner.Rotatable", typeof(BEBehaviorRotatableBanner));
        api.RegisterBlockEntityBehaviorClass("Flags.Banner.WrenchOrientable", typeof(BEBehaviorWrenchOrientableBanner));
        api.RegisterBlockEntityBehaviorClass("Flags.BannerContainable", typeof(BEBehaviorBannerContainable));
        api.RegisterCollectibleBehaviorClass("Flags.BannerPatternName", typeof(CollectibleBehaviorBannerPatternName));
        api.RegisterCollectibleBehaviorClass("Flags.BannerPatternDescription", typeof(CollectibleBehaviorBannerPatternDescription));
        api.RegisterCollectibleBehaviorClass("Flags.BannerPatternToolModes", typeof(CollectibleBehaviorBannerPatternToolModes));
        api.RegisterCollectibleBehaviorClass("Flags.BannerLiquidDescription", typeof(CollectibleBehaviorBannerLiquidDescription));

        api.RegisterCollectibleBehaviorClass("Flags.CutoutTool", typeof(CollectibleBehaviorCutoutTool));
        api.RegisterCollectibleBehaviorClass("Flags.RenameTool", typeof(CollectibleBehaviorRenameTool));

        api.Logger.Event("started '{0}' mod", Mod.Info.Name);

        GlobalConstants.IgnoredStackAttributes = GlobalConstants.IgnoredStackAttributes.Append(BannersIgnoreAttributeSubTrees);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        if (!api.World.Config.HasAttribute(worldConfigLayersLimit))
        {
            api.World.Config.SetInt(worldConfigLayersLimit, defaultLayersLimit);
        }
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Gui.RegisterDialog(new HudElementBannerPreview(api), new HudElementBannerOverview(api), new GuiDialogBannerConfig(api));
        GuiDialogTransformEditor.extraTransforms.Add(new TransformConfig() { Title = langCodeBannerPreviewHudTransform.Localize(), AttributeName = attributeBannerPreviewHudTransform });
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        bool addedCutoutToolToCreative = false;
        bool addedWrenchToolToCreative = false;

        foreach (CollectibleObject obj in api.World.Collectibles)
        {
            obj.PatchLiquidDescription();
            obj.PatchCutoutTool(ref addedCutoutToolToCreative);
            obj.PatchRenameTool();
            obj.PatchWrenchTool(ref addedWrenchToolToCreative);

            if (obj.Code.Equals(itemcodeParchment) || obj.Code.Equals(itemcodeInkAndQuill))
            {
                obj.CreativeInventoryTabs = obj.CreativeInventoryTabs.Append(modCreativeTab);
            }
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        Converter = api.Assets.TryGet(AssetLocation.Create(pathConverter)).ToObject<BannerConverter>();
    }
}