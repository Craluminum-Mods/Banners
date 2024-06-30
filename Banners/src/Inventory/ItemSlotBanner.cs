using Vintagestory.API.Common;

namespace Flags;

public class ItemSlotBanner : ItemSlot
{
    public ItemSlotBanner(InventoryBase inventory) : base(inventory) { }

    public override int MaxSlotStackSize => 1;

    public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
    {
        return sourceSlot?.Itemstack?.Collectible is BlockBanner;
    }

    public override bool CanHold(ItemSlot sourceSlot)
    {
        return sourceSlot?.Itemstack?.Collectible is BlockBanner;
    }
}