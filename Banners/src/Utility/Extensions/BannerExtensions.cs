using SkiaSharp;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.Util;

namespace Flags;

public static class BannerExtensions
{
    public static TextCommandResult GenerateTextures(this TextCommandCallingArgs args)
    {
        bool replaceExisting = args?[0].ToString().ToBool() ?? false;
        ItemSlot slot = args.Caller.Player.Entity.RightHandItemSlot;
        ICoreClientAPI capi = args.Caller.Player.Entity.Api as ICoreClientAPI;
        return slot?.Itemstack?.Collectible is BlockBanner blockBanner
            ? blockBanner.DebugPregenerateTextures(capi, replaceExisting)
            : TextCommandResult.Error($"{modDomain}:command-nobanner".Localize());
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