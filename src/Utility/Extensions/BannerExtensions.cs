using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using System.IO;
using SkiaSharp;
using Vintagestory.API.Config;
using VintagestoryAPI.Util;

namespace Flags;

public static class BannerExtensions
{
    public static MeshData GetOrCreateMesh(this BlockBanner block, ICoreAPI api, BannerProperties properties, ITexPositionSource overrideTexturesource = null)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;

        if (string.IsNullOrEmpty(properties.Placement))
        {
            properties.SetPlacement(block.DefaultPlacement);
        }

        string key = $"{block.Code}-{properties}";

        if (overrideTexturesource != null || !block.Meshes.TryGetValue(key, out MeshData mesh))
        {
            if (!block.CustomShapes.TryGetValue(properties.Placement, out CompositeShape rcshape))
            {
                capi.Tesselator.TesselateBlock(block, out mesh);
                capi.Logger.Error("[Flags] No matching shape found for block {0} for type {1}", block.Code, properties.Placement);
                return mesh;
            }
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Shape shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            ITexPositionSource texSource = overrideTexturesource ?? block.HandleTextures(properties, capi, shape, rcshape.Base.ToString());
            if (shape == null)
            {
                capi.Tesselator.TesselateBlock(block, out mesh);
                capi.Logger.Error("[Flags] Block {0} defines shape '{1}', but no matching shape found", block.Code, rcshape.Base);
                return mesh;
            }
            try
            {
                capi.Tesselator.TesselateShape("Banner block", shape, out mesh, texSource);
            }
            catch (Exception)
            {
                capi.Tesselator.TesselateBlock(block, out mesh);
                capi.Logger.Error("[Flags] Can't create shape for block {0} because of broken textures", block.Code);
                return mesh;
            }
            if (overrideTexturesource == null)
            {
                block.Meshes[key] = mesh;
            }
        }
        return mesh;
    }

    public static MeshData GetOrCreateContainableMesh(this BlockBanner block, ICoreAPI api, ItemStack stack, string shapeKey, Vec3f rotation)
    {
        ICoreClientAPI capi = api as ICoreClientAPI;

        BannerProperties properties = BannerProperties.FromStack(stack);
        string key = $"{block.Code}-{properties}-{shapeKey}-{rotation}";

        if (!block.ContainableMeshes.TryGetValue(key, out MeshData mesh))
        {
            if (!block.CustomShapesContainable.TryGetValueOrWildcard(shapeKey, out CompositeShape rcshape))
            {
                capi.Tesselator.TesselateBlock(block, out mesh);
                capi.Logger.Error("[Flags] No matching shape found for block {0} for BannerContainable key '{1}'", block.Code, shapeKey);
                return mesh;
            }
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Shape shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
            ITexPositionSource texSource = block.HandleTextures(properties, capi, shape, rcshape.Base.ToString());
            if (shape == null)
            {
                capi.Tesselator.TesselateBlock(block, out mesh);
                capi.Logger.Error("[Flags] BannerContainable {0} defines shape '{1}', but no matching shape found", block.Code, rcshape.Base);
                return mesh;
            }
            try
            {
                capi.Tesselator.TesselateShape("Containable banner block", shape, out mesh, texSource, rotation);
            }
            catch (Exception)
            {
                capi.Tesselator.TesselateBlock(block, out mesh);
                capi.Logger.Error("[Flags] Can't create shape for block {0} for BannerContainable key '{1}' because of broken textures", block.Code, shapeKey);
                return mesh;
            }
            block.ContainableMeshes[key] = mesh;
        }
        return mesh;
    }

    public static void GetInventoryMesh(this BlockBanner block, ICoreClientAPI capi, ItemStack stack, ItemRenderInfo renderinfo)
    {
        BannerProperties properties = BannerProperties.FromStack(stack, block);
        string key = $"{block.Code}-{properties}";
        if (!block.InvMeshes.TryGetValue(key, out MultiTextureMeshRef meshref))
        {
            MeshData mesh = block.GetOrCreateMesh(capi, properties);
            meshref = block.InvMeshes[key] = capi.Render.UploadMultiTextureMesh(mesh);
        }
        renderinfo.ModelRef = meshref;
    }

    public static ITexPositionSource HandleTextures(this BlockBanner block, BannerProperties properties, ICoreClientAPI capi, Shape shape, string filenameForLogging = "")
    {
        ShapeTextureSource texSource = new ShapeTextureSource(capi, shape, filenameForLogging);

        foreach ((string textureCode, CompositeTexture texture) in block.CustomTextures)
        {
            CompositeTexture ctex = texture.Clone();

            if (block.TextureCodesForOverlays.Contains(textureCode))
            {
                foreach (BannerLayer layer in properties.Patterns.GetOrdered(textureCode))
                {
                    block.ApplyOverlay(capi, textureCode, ctex, layer);
                }

                foreach (BannerLayer layer in properties.Cutouts.GetOrdered(textureCode))
                {
                    block.ApplyOverlay(capi, textureCode, ctex, layer, EnumColorBlendMode.ColorBurn);
                }
            }

            ctex.Bake(capi.Assets);
            texSource.textures[textureCode] = ctex;
        }
        return texSource;
    }

    public static void ApplyOverlay(this BlockBanner block, ICoreClientAPI capi, string textureCode, CompositeTexture ctex, BannerLayer layer, EnumColorBlendMode blendMode = EnumColorBlendMode.Normal)
    {
        if ((block.IgnoredTextureCodes.TryGetValue(textureCode, out List<string> ignoredTextureCodes) && ignoredTextureCodes.Contains(layer.Pattern)) == true)
        {
            return;
        }
        ctex.BlendedOverlays ??= Array.Empty<BlendedOverlayTexture>();
        if (!block.CustomTextures.TryGetValue(layer.TextureCode, out CompositeTexture _overlayTexture) || _overlayTexture == null)
        {
            capi.Logger.Error("[Flags] Block {0} defines an overlay texture key '{1}', but no matching texture found", block.Code, layer.TextureCode);
            ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = AssetLocation.Create(textureUnknown), BlendMode = blendMode });
            return;
        }

        CompositeTexture overlayTexture = _overlayTexture.Clone();
        overlayTexture.FillPlaceholder(textureCodeColor, layer.Color ?? "black");
        overlayTexture.FillPlaceholder(textureCodePattern, layer.Pattern);

        AssetLocation logCode = overlayTexture.Base.Clone().WithPathPrefixOnce(prefixTextures).WithPathAppendixOnce(appendixPng);
        if (!capi.Assets.Exists(logCode))
        {
            capi.Logger.Error("[Flags] Block {0} defines an overlay texture key '{1}' with path '{2}' for color '{3}', but no matching texture found", block.Code, layer.TextureCode, logCode.ToString(), layer.Color ?? "black");
            ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = AssetLocation.Create(textureUnknown), BlendMode = blendMode });
            return;
        }

        ctex.BlendedOverlays = ctex.BlendedOverlays.Append(new BlendedOverlayTexture() { Base = overlayTexture.Base, BlendMode = blendMode });
    }

    public static TextCommandResult DebugPregenerateTextures(this BlockBanner blockBanner, ICoreClientAPI capi, bool replaceExisting = false, string grayscaleColor = defaultColor)
    {
        int amount = 0;
        foreach ((string key, CompositeTexture tex) in blockBanner.CustomTextures)
        {
            if (blockBanner.IgnoreForGeneratingTextures.Contains(key) || (!replaceExisting && capi.Assets.Exists(tex.Base)))
            {
                continue;
            }

            foreach (string colorKey in blockBanner.Colors)
            {
                AssetLocation location = tex.Base.Clone().WithPathPrefixOnce(prefixTextures).WithPathAppendixOnce(appendixPng);

                AssetLocation newLocation = location.Clone();
                newLocation.Path = newLocation.Path.Replace(textureCodeColor, colorKey);
                if (!replaceExisting && capi.Assets.Exists(newLocation))
                {
                    continue;
                }

                IAsset coloredTextureAsset = capi.Assets.TryGet(AssetLocation.Create($"{blockBanner.TopTexturePrefix}{colorKey}").WithPathPrefixOnce(prefixTextures).WithPathAppendixOnce(appendixPng));
                if (coloredTextureAsset == null)
                {
                    continue;
                }

                AssetLocation _tempAlphaLocation = location.Clone();
                _tempAlphaLocation.Path = _tempAlphaLocation.Path.Replace(textureCodeColor, grayscaleColor);
                IAsset alphaTexAsset = capi.Assets.TryGet(_tempAlphaLocation);
                if (alphaTexAsset == null)
                {
                    continue;
                }

                BitmapRef alphaBmp = alphaTexAsset.ToBitmap(capi);
                BitmapRef coloredTextureBmp = coloredTextureAsset.ToBitmap(capi);
                SKBitmap layerBmp = new SKBitmap(alphaBmp.Width, alphaBmp.Height);

                int[] layerPixels = new int[alphaBmp.Pixels.Length];
                for (int i = 0; i < layerPixels.Length; i++)
                {
                    int coloredPixel = coloredTextureBmp.Pixels[i];
                    int alphaPixel = alphaBmp.Pixels[i];

                    SKColor color = new SKColor((byte)((coloredPixel >> 16) & 0xFF),
                        (byte)((coloredPixel >> 8) & 0xFF),
                        (byte)((coloredPixel) & 0xFF),
                        (byte)((alphaPixel >> 24) & 0xFF));

                    layerPixels[i] = color.ToArgb();
                }

                layerBmp.SetPixels(layerPixels);

                string newFilePath = Path.Combine(GamePaths.Cache, outputFolderName, location.Domain, newLocation.Clone().Path.Replace('/', Path.DirectorySeparatorChar));
                string newFolderPath = newFilePath.RemoveAfterLastSymbol(Path.DirectorySeparatorChar);
                if (!Path.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                }
                if (replaceExisting || !File.Exists(newFilePath))
                {
                    layerBmp.Save(newFilePath);
                    amount++;
                }
            }
        }
        return amount > 0
            ? TextCommandResult.Success($"{modDomain}:command-generatetextures".Localize(amount.ToString()))
            : TextCommandResult.Error($"{modDomain}:command-generatetextures-error".Localize());
    }
}