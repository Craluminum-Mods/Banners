using System;

namespace Flags;

public static class Constants
{
    public const string modDomain = "flags";

    public const float DegreesToRadians = (float)Math.PI / 180f;
    public const float RadiansToDegrees = 180f / (float)Math.PI;

    public const float Radians90 = (float)Math.PI / 2f;
    public const float Radians22_5 = (float)Math.PI / 8f;

    public const string Wildcard = "*";
    public const char layerSeparator = '|';
    public const int bannerCodeMaxElements = 2;

    public const string worldConfigLayersLimit = "bannerLayersLimit";
    public const int defaultLayersLimit = 14;

    public const string textureCodeColor = "{color}";
    public const string textureCodePattern = "{pattern}";
    public const string textureUnknown = "unknown";

    public const string langCodeBannerLiquidType = $"{modDomain}:BannerLiquid.Type";
    public const string langCodeBannerColorType = $"{modDomain}:BannerLiquid.Color";

    public const string langCodeColor = "color-";
    public const string langCodePattern = $"{modDomain}:Pattern.";
    public const string langCodePatternAndColor = $"{modDomain}:pattern-and-color";

    public const string langCodeAddLayer = $"{modDomain}:blockhelp-banner-addlayer";
    public const string langCodeRemovelayer = $"{modDomain}:blockhelp-banner-removelayer";
    public const string langCodeCopyLayers = $"{modDomain}:blockhelp-banner-copylayers";
    public const string langCodeRename = $"{modDomain}:blockhelp-banner-rename";
    public const string langCodeRotate = "Rotate";
    public const string langCodeRotateByAxis = $"{modDomain}:Rotate.Axis";
    public const string langCodeRotateBy22_5 = $"{modDomain}:Rotate.22.5";
    public const string langCodeRotateBy90 = $"{modDomain}:Rotate.90";
    public const string langCodeClearRotationsXZ = $"{modDomain}:ClearRotationsXZ";
    public const string langCodeCycleVariants = $"{modDomain}:blockhelp-banner-cyclevariants";

    public const string cacheKeyBlockBannerMeshes = $"{modDomain}BlockBannerMeshes";
    public const string cacheKeyBlockBannerInvMeshes = $"{modDomain}BlockBannerMeshesInventory";
    public const string cacheKeyItemBannerPatternMeshes = $"{modDomain}ItemBannerPatternMeshes";
    public const string cacheKeyItemBannerPatternMeshesInv = $"{modDomain}ItemBannerPatternMeshesInventory";
    public const string cacheKeyBannerInteractions = $"{modDomain}BannerInteractions";

    public const string attributeBanner = "banner";
    public const string attributeLayers = "layers";
    public const string attributeName = "name";
    public const string attributePlacement = "placement";
    public const string attributeBannerPattern = "bannerpattern";
    public const string attributeType = "type";
    public const string attributeRotateX = "rotateX";
    public const string attributeRotateY = "meshAngle";
    public const string attributeRotateZ = "rotateZ";
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

    public const string appendixJson = ".json";
    public const string appendixPng = ".png";
    public const string prefixShapes = "shapes/";
    public const string prefixTextures = "textures/";

    public class Commands
    {
        public class GenerateTextures
        {
            public const string Description = $"{modDomain}:Command.GenerateTextures.Description";
            public const string Success = $"{modDomain}:Command.GenerateTextures.Success";
            public const string ErrorNoBanner = $"{modDomain}:Command.GenerateTextures.Error.NoBanner";
            public const string OptionalArgGrayscaleColor = $"{modDomain}:Command.GenerateTextures.OptionalArg.GrayscaleColor";
            public const string OptionalArgReplaceExisting = $"{modDomain}:Command.GenerateTextures.OptionalArg.ReplaceExisting";
        }
    }

    public class IngameError
    {
        public const string BannerCopyLayers = $"{modDomain}:ingameerror-banner-copylayers";
        public const string BannerRename = $"{modDomain}:ingameerror-banner-rename";
    }
}
