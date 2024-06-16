using System;
using System.IO;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace Flags;

public static class Constants
{
    public const int bannerCodeMaxElements = 2;
    public const int defaultLayersLimit = 14;
    public const int lastPatternDisplayAmount = 8;
    public const int cutoutsDisplayAmount = 4;

    public const float DegreesToRadians = (float)Math.PI / 180f;
    public const float RadiansToDegrees = 180f / (float)Math.PI;
    public const float Radians90 = (float)Math.PI / 2f;
    public const float Radians22_5 = (float)Math.PI / 8f;

    public const char unlockedSeparator = '-';
    public const char layerSeparator = '|';
    public const string commaSeparator = ", ";

    public const string modDomain = "flags";
    public const string modCreativeTab = "flags";
    public const string Wildcard = "*";
    public const string worldConfigLayersLimit = "bannerLayersLimit";
    public const string textureCodeColor = "{color}";
    public const string textureCodePattern = "{pattern}";
    public const string textureUnknown = "unknown";
    public const string defaultColor = "grayscale";

    public const string langCodeEmpty = "Empty";
    public const string langCodeColor = "color-";
    public const string langCodeSetToolmode = "heldhelp-settoolmode";
    public const string langCodeRightClickPickUp = "blockhelp-behavior-rightclickpickup";
    public const string langCodePattern = $"{modDomain}:pattern-";
    public const string langCodePatternDesc = $"{modDomain}:patterndesc-";
    public const string langCodeBannerLiquidType = $"{modDomain}:banner-liquid-type";
    public const string langCodeBannerColorType = $"{modDomain}:banner-liquid-color";
    public const string langCodePatternGroup = $"{modDomain}:patterngroup-";
    public const string langCodePatternGroups = $"{modDomain}:patterngroups";
    public const string langCodeAddLayer = $"{modDomain}:blockhelp-banner-addlayer";
    public const string langCodeRemoveLayer = $"{modDomain}:blockhelp-banner-removelayer";
    public const string langCodeCopyLayers = $"{modDomain}:blockhelp-banner-copylayers";
    public const string langCodeCopyLayersFromPlaced = $"{modDomain}:blockhelp-banner-copylayers-placed-to-held";
    public const string langCodeRename = $"{modDomain}:blockhelp-banner-rename";
    public const string langCodeSwapModel = $"{modDomain}:blockhelp-banner-swapmodel";
    public const string langCodeRotateBy22_5 = $"{modDomain}:blockhelp-banner-rotate-22_5";
    public const string langCodeRotateByAxisBy90 = $"{modDomain}:blockhelp-banner-rotate-axis-90";
    public const string langCodeClearRotationsXZ = $"{modDomain}:blockhelp-banner-clear-rotations-xz";
    public const string langCodeBannerContainableContainedBanner = $"{modDomain}:bannercontainable-contained-banner";
    public const string langCodeBannerContainableContainedBannerAdd = $"{modDomain}:blockhelp-bannercontainable-add";
    public const string langCodeBannerContainableContainedBannerRemove = $"{modDomain}:blockhelp-bannercontainable-remove";
    public const string langCodeBannerBannerPatternSet = $"{modDomain}:heldhelp-bannerpattern-set";
    public const string langCodePatternLocked = $"{modDomain}:discoverable-pattern";
    public const string langCodeUnlockedPatterns = $"{modDomain}:unlocked-patterns";
    public const string langCodePatterns = $"{modDomain}:patterns";
    public const string langCodeCutouts = $"{modDomain}:cutouts";
    public const string langCodeBannerModes = $"{modDomain}:banner-modes";
    public const string langCodeAddCutout = $"{modDomain}:blockhelp-banner-addcutout";
    public const string langCodeRemoveCutout = $"{modDomain}:blockhelp-banner-removecutout";
    public const string langCodeToolMode = $"{modDomain}:toolmode-";
    public const string langCodeToolModeValue = $"{modDomain}:toolmode-value-";

    public const string cacheKeyBlockBannerMeshes = $"{modDomain}BlockBannerMeshes";
    public const string cacheKeyBlockBannerInvMeshes = $"{modDomain}BlockBannerMeshesInventory";
    public const string cacheKeyItemBannerPatternMeshes = $"{modDomain}ItemBannerPatternMeshes";
    public const string cacheKeyItemBannerPatternMeshesInv = $"{modDomain}ItemBannerPatternMeshesInventory";
    public const string cacheKeyBannerInteractions = $"{modDomain}BannerInteractions";
    public const string cacheKeyBlockBannerContainableMeshes = $"{modDomain}BlockBannerMeshesContainable";
    public const string cacheKeyBookStacks = $"{modDomain}BookStacks";
    public const string cacheKeyWrenchStacks = $"{modDomain}WrenchStacks";
    public const string cacheKeyDyeStacks = $"{modDomain}DyeStacks";
    public const string cacheKeyBleachStacks = $"{modDomain}BleachStacks";
    public const string cacheKeyBannerStacks = $"{modDomain}BannerStacks";
    public const string cacheKeyShearsStacks = $"{modDomain}ShearsStacks";
    public const string cacheKeyRotatableBannerInteractions = $"{modDomain}RotatableBannerInteractions";
    public const string cacheKeyWrenchableBannerInteractions = $"{modDomain}WrenchableBannerInteractions";
    public const string cacheKeyQuestionTexture = $"{modDomain}QuestionTexture";
    public const string cacheKeyBannerToolModeTextures = $"{modDomain}BannerToolModeTextures";

    public const string attributeTitle = "title";
    public const string attributeBanner = "banner";
    public const string attributeLayers = "layers";
    public const string attributeCutouts = "cutouts";
    public const string attributeName = "name";
    public const string attributePlacement = "placement";
    public const string attributeBannerPattern = "bannerpattern";
    public const string attributeType = "type";
    public const string attributeRotateX = "rotateX";
    public const string attributeRotateY = "meshAngle";
    public const string attributeRotateZ = "rotateZ";
    public const string attributePatternGroups = "patternGroups";
    public const string attributeShapes = "shapes";
    public const string attributeTextures = "textures";
    public const string attributeColors = "colors";
    public const string attributeTopTexturePrefix = "topTexturePrefix";
    public const string attributeShouldGenerateTextures = "shouldGenerateTextures";
    public const string attributeIgnoredTextureCodesForGeneratingTextures = "ignoredTextureCodesForGeneratingTextures";
    public const string attributeGrayscaleColor = "grayscaleColor";
    public const string attributeGenerateForExistingTextures = "generateForExistingTextures";
    public const string attributeIgnoredTextureCodesForOverlays = "ignoredTextureCodesForOverlays";
    public const string attributeTextureCodesForOverlays = "textureCodesForOverlays";
    public const string attributeSelectionBoxes = "selectionBoxes";
    public const string attributeCollisionBoxes = "collisionBoxes";
    public const string attributeDefaultPlacement = "defaultPlacement";
    public const string attributeDefaultHorizontalPlacement = "defaultHorizontalPlacement";
    public const string attributeDefaultVerticalPlacement = "defaultVerticalPlacement";
    public const string attributeShowDebugInfo = "showDebugInfo";
    public const string attributeParts = "parts";
    public const string attributeReplacePart = "replacePart";
    public const string attributeDefaultName = "defaultName";
    public const string attributePlacements = "placements";
    public const string attributePlacementBaseCodes = "placementBaseCodes";
    public const string attributeDefaultType = "defaultType";
    public const string attributeRolledShape = "rolledShape";
    public const string attributeBannerLiquid = "bannerLiquid";
    public const string attributeShapesContainable = "shapesContainable";
    public const string attributeInventoryBannerContainable = "inventoryBannerContainable";
    public const string attributeExcludeFaces = "excludeFaces";
    public const string attributeShapeKey = "shapeKey";
    public const string attributeRotationsByFace = "rotationsByFace";
    public const string attributeToolModes = "toolModes";
    public const string attributeBannerModes = "bannerModes";
    public const string attributeDefaultModes = "defaultModes";
    public const string attributeUnlockedTypes = "unlockedTypes";
    public const string attributeEnabled = "enabled";
    public const string attributeRotX = "rotX";
    public const string attributeRotY = "rotY";
    public const string attributeRotZ = "rotZ";

    public const string hotkeyToolmodeSelect = "toolmodeselect";
    public const string eventKeepOpenToolmodeDialog = "keepopentoolmodedlg";

    public const string bannerContainableInvClassName = "flags-bannercontainable";

    public const string appendixJson = ".json";
    public const string appendixPng = ".png";
    public const string prefixShapes = "shapes/";
    public const string prefixTextures = "textures/";

    public const string pathConverter = "flags:config/mc-to-vs-converter.json";
    public const string outputFolderName = "Banners";
    public static string OutputFolder => Path.Combine(GamePaths.Cache, outputFolderName);

    public static string[] BannersIgnoreAttributeSubTrees => new string[] { attributeBannerModes };

    public class IngameError
    {
        public const string Prefix = $"{modDomain}:ingameerror-";

        public const string BannerCopyLayers = $"{Prefix}banner-copylayers";
        public const string BannerRename = $"{Prefix}:ingameerror-banner-rename";
        public const string BannerPatternGroups = $"{Prefix}banner-patterngroups";
        public const string BannerNotEnoughDye = $"{Prefix}banner-notenoughdye";
        public const string BannerNotEnoughBleach = $"{Prefix}banner-notenoughbleach";
        public const string LayersLimitReached = $"{Prefix}layers-limitreached";
        public const string LiquidContainerOneMax = $"{Prefix}liquidcontainer-onemax";
    }

    public class IntColor
    {
        public static readonly int Gray = ColorUtil.Hex2Int("#a9a9a9");
    }
}
