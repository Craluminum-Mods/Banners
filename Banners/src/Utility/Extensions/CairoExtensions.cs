using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Flags;

public static class CairoExtensions
{
    public static void DrawChevron(this Context ctx, ImageSurface surface, ElementBounds currentBounds, double iconSize, double[] color = null, double[] strokeColor = null, double iconStrokeSize = 1)
    {
        ctx.Save();
        ctx.Scale(GuiElement.scaled(iconSize), GuiElement.scaled(iconSize));
        ctx.MoveTo(0, 0.12);
        ctx.LineTo(0.1724, 0.5);
        ctx.LineTo(0, 0.8793);
        ctx.LineTo(0.3793, 0.8793);
        ctx.LineTo(0.5517, 0.5);
        ctx.LineTo(0.3793, 0.12);
        ctx.ClosePath();
        ctx.MoveTo(0.4483, 0.1207);
        ctx.LineTo(0.6207, 0.5);
        ctx.LineTo(0.4483, 0.8793);
        ctx.LineTo(0.8276, 0.8793);
        ctx.LineTo(1, 0.5);
        ctx.LineTo(0.8276, 0.1207);
        ctx.ClosePath();
        if (color != null)
        {
            ctx.SetSourceRGBA(color);
        }
        ctx.FillPreserve();
        ctx.Restore();
        if (strokeColor != null)
        {
            ctx.SetSourceRGBA(strokeColor);
            ctx.Scale(iconStrokeSize, iconStrokeSize);
            ctx.Stroke();
        }
    }

    public static LoadedTexture DrawLetterIcon(this ICoreClientAPI capi, string letter, string hexColor)
    {
        if (capi == null)
        {
            return null;
        }
        int isize = (int)GuiElement.scaled(48.0);
        return capi.Gui.Icons.GenTexture(isize, isize, delegate (Context ctx, ImageSurface surface)
        {
            CairoFont cairoFont = CairoFont.WhiteMediumText().WithColor(new double[4] { 1.0, 1.0, 1.0, 1.0 });
            cairoFont.SetupContext(ctx);
            ctx.SetSourceRGBA(ColorUtil.Hex2Doubles(hexColor));
            TextExtents textExtents = cairoFont.GetTextExtents(letter);
            double num = cairoFont.GetFontExtents().Ascent + GuiElement.scaled(2.0);
            capi.Gui.Text.DrawTextLine(ctx, cairoFont, letter, ((double)isize - textExtents.Width) / 2.0, ((double)isize - num) / 2.0);
        });
    }
}