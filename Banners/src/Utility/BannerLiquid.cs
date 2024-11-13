using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

public enum EnumBannerLiquid
{
    Dye = 0,
    Bleach = 1
}

public class BannerLiquid
{
    [JsonProperty]
    public EnumBannerLiquid Type { get; set; } = EnumBannerLiquid.Dye;

    public float LitresPerUse { get; set; } = 0.1f;
    public string Color { get; set; }

    public bool IsDye => Type == EnumBannerLiquid.Dye && !string.IsNullOrEmpty(Color);
    public bool IsBleach => Type == EnumBannerLiquid.Bleach;

    public bool CanTakeLiquid(ItemStack containerStack, BlockLiquidContainerTopOpened blockContainer)
    {
        return blockContainer.GetCurrentLitres(containerStack) >= LitresPerUse / (float)containerStack.StackSize;
    }

    public bool TryTakeLiquid(IPlayer byPlayer, ItemSlot slot, BlockLiquidContainerTopOpened blockContainer)
    {
        WaterTightContainableProps owncontentProps = blockContainer.GetContentProps(slot.Itemstack);
        ItemStack owncontentStack = blockContainer.GetContent(slot.Itemstack);
        ItemStack liquidStackForParticles = owncontentStack.Clone();
        int moved = 0;
        if (byPlayer.IsCreative())
        {
            moved = 1000;
        }
        else
        {
            moved = blockContainer.SplitStackAndPerformAction(byPlayer.Entity, slot, (ItemStack stack) =>
            {
                return blockContainer.TryTakeLiquid(slot.Itemstack, LitresPerUse)?.StackSize * (int)owncontentProps.ItemsPerLitre ?? 0;
            });
        }
        if (moved > 0)
        {
            blockContainer.DoLiquidMovedEffects(byPlayer, liquidStackForParticles, moved, BlockLiquidContainerBase.EnumLiquidDirection.Pour);
            return true;
        }
        return false;
    }

    public static bool TryGet(ItemStack fromStack, BlockLiquidContainerTopOpened container, out BannerLiquid props)
    {
        return TryGet(container.GetContent(fromStack)?.Collectible, out props);
    }

    public static bool TryGet(CollectibleObject obj, out BannerLiquid props)
    {
        props = null;

        if (HasAttribute(obj))
        {
            props = obj.Attributes[attributeBannerLiquid].AsObject<BannerLiquid>();
            return props != null;
        }

        return false;
    }

    public static bool HasAttribute(CollectibleObject obj)
    {
        return obj?.Attributes?.KeyExists(attributeBannerLiquid) == true;
    }
}