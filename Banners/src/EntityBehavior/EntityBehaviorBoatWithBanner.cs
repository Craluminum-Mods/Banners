using System;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Flags;

public class EntityBehaviorBoatWithBanner : EntityBehavior
{
    private InventoryGeneric inv;

    public EntityBehaviorBoatWithBanner(Entity entity) : base(entity) { }

    public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
    {
        int quantitySlots = typeAttributes[attributeQuantitySlots].AsInt(1);

        inv = new InventoryGeneric(quantitySlots, bannerContainableBoatInvClassName + "-" + entity.EntityId, entity.Api, (int id, InventoryGeneric inv) => new ItemSlotBanner(inv));
        if (entity.WatchedAttributes[attributeInventoryBannerContainableBoat] is TreeAttribute tree)
        {
            inv.FromTreeAttributes(tree);
        }
        if (entity.World.Side == EnumAppSide.Server)
        {
            inv.SlotModified += Inv_SlotModified;
        }

        (entity.Api as ICoreClientAPI)?.Event.RegisterEventBusListener((string eventName, ref EnumHandling handling, IAttribute data) =>
        {
            switch (eventName)
            {
                case eventOnCloseEditTransforms:
                case eventOnEditTransforms:
                case eventOnApplyTransforms:
                case eventGenJsonTransform:
                    if (entity.Code.Domain == "game")
                    {
                        (entity.Properties.Client.Renderer as EntityShapeRenderer)?.MarkShapeModified();
                    }
                    break;
                }
        });
    }

    private void Inv_SlotModified(int slotid)
    {
        ITreeAttribute tree = new TreeAttribute();
        entity.WatchedAttributes[attributeInventoryBannerContainableBoat] = tree;
        entity.WatchedAttributes.MarkPathDirty(attributeInventoryBannerContainableBoat);
        inv.ToTreeAttributes(tree);
        if (entity.Api is ICoreServerAPI coreServerAPI)
        {
            coreServerAPI.Network.BroadcastEntityPacket(entity.EntityId, 1235, SerializerUtil.ToBytes(tree.ToBytes));
        }
    }

    public override void OnReceivedServerPacket(int packetid, byte[] data, ref EnumHandling handled)
    {
        if (packetid == 1235)
        {
            TreeAttribute tree = new TreeAttribute();
            SerializerUtil.FromBytes(data, tree.FromBytes);
            inv.FromTreeAttributes(tree);
            if (entity.Code.Domain == "game") (entity.Properties.Client.Renderer as EntityShapeRenderer)?.MarkShapeModified();
        }
    }

    public override void OnEntityDeath(DamageSource damageSourceForDeath)
    {
        if (entity.World.Side == EnumAppSide.Server)
        {
            inv?.DropAll(entity.ServerPos.XYZ);
        }
    }

    public override void OnEntityDespawn(EntityDespawnData despawn)
    {
        if (entity.World.Side == EnumAppSide.Server)
        {
            inv?.DropAll(entity.ServerPos.XYZ);
        }
    }

    public override WorldInteraction[] GetInteractionHelp(IClientWorldAccessor world, EntitySelection es, IClientPlayer player, ref EnumHandling handled)
    {
        handled = EnumHandling.PassThrough;
        return new WorldInteraction[] {
            new WorldInteraction()
            {
                ActionLangCode = langCodeBannerContainableContainedBannerAdd,
                MouseButton = EnumMouseButton.Right,
                HotKeyCode = "shift",
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(world.Api, cacheKeyBannerStacks)
            },
            new WorldInteraction()
            {
                ActionLangCode = langCodeBannerContainableContainedBannerRemove,
                RequireFreeHand = true,
                HotKeyCode = "shift",
                MouseButton = EnumMouseButton.Right
            },
        };
    }

    public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode, ref EnumHandling handled)
    {
        if (entity.World.Side != EnumAppSide.Server)
        {
            return;
        }

        switch (entity.Code.Domain)
        {
            case "sailboat" when byEntity.ServerControls.ShiftKey:
                {
                    bool success = TryPut(itemslot) || TryTake();
                    handled = success ? EnumHandling.PreventDefault : EnumHandling.PassThrough;
                    break;
                }
            case "game" when byEntity.ServerControls.CtrlKey:
                {
                    bool success = TryPut(itemslot) || TryTake();
                    handled = success ? EnumHandling.PreventDefault : EnumHandling.PassThrough;
                    break;
                }
        }
    }

    public override void GetInfoText(StringBuilder infotext)
    {
        if (inv == null)
        {
            return;
        }

        foreach (ItemSlot slot in inv)
        {
            infotext.AppendLine(!slot.Empty ? langCodeBannerContainableContainedBanner.Localize(slot.Itemstack.GetName()) : langCodeEmpty.Localize());
        }
    }

    public void AddMeshDataTo(ref MeshData meshData)
    {
        if (inv == null || inv.Empty)
        {
            return;
        }

        foreach (ItemSlot slot in inv.Where(slot => !slot.Empty))
        {
            if (slot.Itemstack.Collectible is not BlockBanner blockBanner)
            {
                continue;
            }
            BannerProperties bannerProperties = BannerProperties.FromStack(slot.Itemstack);
            bannerProperties.SetPlacement(GetPlacement(blockBanner, entity));
            MeshData mesh = blockBanner.GetOrCreateMesh(entity.Api as ICoreClientAPI, bannerProperties).Clone();
            if (mesh == null)
            {
                continue;
            }

            mesh.MatrixTransform(GetTransformation(blockBanner, entity).AsMatrix);
            mesh.ClearWindFlags();
            meshData.AddMeshData(mesh);
        }
    }

    public static ModelTransform GetTransformation(BlockBanner forBanner, Entity forEntity, ItemSlot forSlot = null, InventoryGeneric inventory = null)
    {
        ModelTransform transform = forBanner.BannerOnBoatTransform;
        if (forBanner.BannerOnBoatTransformByBoat.Any(x => WildcardUtil.Match(x.Key, forEntity.Code.ToString()) && x.Value != null))
        {
            ModelTransform _transform = forBanner.BannerOnBoatTransformByBoat.First(x => WildcardUtil.Match(x.Key, forEntity.Code.ToString())).Value;
            if (_transform != null) transform = _transform;
        }
        return transform;
    }

    public static string GetPlacement(BlockBanner forBanner, Entity forEntity, ItemSlot forSlot = null, InventoryGeneric inventory = null)
    {
        string placement = forBanner.DefaultPlacement;
        if (forBanner.PlacementsByBoat.Any(x => WildcardUtil.Match(x.Key, forEntity.Code.ToString()) && x.Value != null))
        {
            string _placement = forBanner.PlacementsByBoat.First(x => WildcardUtil.Match(x.Key, forEntity.Code.ToString())).Value;
            if (_placement != null) placement = _placement;
        }
        return placement;
    }

    public bool TryPut(ItemSlot slot)
    {
        if (inv == null || !inv.Any(slot => slot.Empty))
        {
            return false;
        }

        int num = slot.TryPutInto(entity.Api.World, inv.First(slot => slot.Empty));
        return num > 0;
    }

    public bool TryTake()
    {
        if (inv == null || inv.FirstNonEmptySlot == null)
        {
            return false;
        }
        inv.DropSlots(entity.SidedPos.AsBlockPos.ToVec3d().Add(0.5, 0.5, 0.5), inv.GetSlotId(inv.FirstNonEmptySlot));
        return true;
    }

    public override string PropertyName()
    {
        return "flags:bannercontainableboat";
    }
}
