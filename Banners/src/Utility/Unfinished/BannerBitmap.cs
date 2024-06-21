using SkiaSharp;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using VintagestoryAPI.Util;

namespace Flags;

/// <summary> Not used </summary>
public class BannerBitmap : IBitmap
{
    int width = 32;
    int height = 32;

    public int Width => width;

    public int Height => height;

    public SKBitmap bitmap;
    public SKBitmap alphaBitmap;

    public int[] Pixels => GetBitmapAsInts();

    public BannerBitmap()
    {
        bitmap = new SKBitmap(width, height);
    }

    public SKColor GetPixel(int x, int y)
    {
        SKColor col = bitmap.GetPixel(Math.Min(x, bitmap.Width - 1), Math.Min(y, bitmap.Height - 1));
        col = col.WithAlpha(alphaBitmap.GetPixel(Math.Min(x, alphaBitmap.Width - 1), Math.Min(y, alphaBitmap.Height - 1)).Red);

        return col;
    }

    public SKColor GetPixelRel(float x, float y)
    {
        return GetPixel((int)((float)width * x), (int)((float)height * x));
    }

    public int[] GetPixelsTransformed(int rot = 0, int alpha = 100)
    {
        return GetBitmapAsInts();
    }

    int[] GetBitmapAsInts()
    {
        List<int> pixels = new List<int>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pixels.Add(GetPixel(x, y).ToArgb());
            }
        }
        return pixels.ToArray();
    }

    public void SetBitmap(SKBitmap bmp)
    {
        width = bmp.Width;
        height = bmp.Height;

        bitmap = bmp;
    }

    public void SetAlphaBitmap(SKBitmap bmp)
    {
        alphaBitmap = bmp;
    }
}