using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace Flags;

public class CollectibleBehaviorCutoutTool : CollectibleBehavior
{
    public bool IsTool { get; protected set; } = true;

    public int ToolDurabilityCost { get; protected set; } = 1;

    public AssetLocation CutSound { get; protected set; } = AssetLocation.Create(DefaultSoundCutoutAdd);

    public AssetLocation RepairSound { get; protected set; } = AssetLocation.Create(DefaultSoundCutoutRemove);

    public CollectibleBehaviorCutoutTool(CollectibleObject obj) : base(obj) { }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        IsTool = properties[attributeIsTool].AsBool(true);
        ToolDurabilityCost = properties[attributeToolDurabilityCost].AsInt(1);

        if (properties.KeyExists(attributeCutSound))
        {
            CutSound = AssetLocation.Create(properties[attributeCutSound].AsString(DefaultSoundCutoutAdd));
        }
        if (properties.KeyExists(attributeRepairSound))
        {
            RepairSound = AssetLocation.Create(properties[attributeRepairSound].AsString(DefaultSoundCutoutRemove));
        }
    }

    public bool ConsumeDurability(IWorldAccessor world, Entity byEntity, ItemSlot slot)
    {
        if ((byEntity as EntityPlayer)?.Player.IsCreative() == true)
        {
            return true;
        }
        if (IsTool)
        {
            int remainingDurabilityBefore = slot.Itemstack.Collectible.GetRemainingDurability(slot.Itemstack);
            slot?.Itemstack?.Collectible.DamageItem(world, byEntity, slot, ToolDurabilityCost);
            int remainingDurabilityAfter = slot?.Itemstack?.Collectible?.GetRemainingDurability(slot?.Itemstack) ?? 0;
            return remainingDurabilityBefore > remainingDurabilityAfter;
        }
        return true;
    }

    public bool HasEnoughDurability(ItemSlot slot)
    {
        int remainingDurabilityBefore = slot.Itemstack.Collectible.GetRemainingDurability(slot.Itemstack);
        return (remainingDurabilityBefore - ToolDurabilityCost) >= 0;
    }

    public void PlayCutSound(IPlayer player)
    {
        player.Entity.World.PlaySoundAt(CutSound, player.Entity, player, randomizePitch: true, 16f);
    }

    public void PlayRepairSound(IPlayer player)
    {
        player.Entity.World.PlaySoundAt(RepairSound, player.Entity, player, randomizePitch: true, 16f);
    }
}