using Vintagestory.API.Client;

namespace Flags;

public static class GuiElementExtensions
{
    public static double Scaled(this double value)
    {
        return GuiElement.scaled(value);
    }

    public static double Scaledi(this double value)
    {
        return GuiElement.scaledi(value);
    }

    public static ElementBounds BelowCopySet(ref ElementBounds bounds, double fixedDeltaX = 0.0, double fixedDeltaY = 0.0, double fixedDeltaWidth = 0.0, double fixedDeltaHeight = 0.0)
    {
        return bounds = bounds.BelowCopy(fixedDeltaX, fixedDeltaY, fixedDeltaWidth, fixedDeltaHeight);
    }
}