using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

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
                Itemstacks = ObjectCacheUtil.TryGet<ItemStack[]>(world.Api, cacheKeyBannerStacks)
            },
            new WorldInteraction()
            {
                ActionLangCode = langCodeBannerContainableContainedBannerRemove,
                MouseButton = EnumMouseButton.Left
            },
        };
    }

    public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode, ref EnumHandling handled)
    {
        IPlayer player = entity.World.PlayerByUid((byEntity as EntityPlayer).PlayerUID);
        if (entity.World.Side == EnumAppSide.Server && byEntity.ServerControls.Sneak)
        {
           TryPut(itemslot);
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
            MeshData mesh = blockBanner.GetOrCreateMesh(entity.Api as ICoreClientAPI, bannerProperties);
            if (mesh == null)
            {
                continue;
            }
            meshData.AddMeshData(mesh);
        }
    }

    public bool TryPut(ItemSlot slot)
    {
        if (inv != null && inv.Any(slot => slot.Empty))
        {
            int num = slot.TryPutInto(entity.Api.World, inv.First(slot => slot.Empty));
            return num > 0;
        }
        return false;
    }

    public override string PropertyName()
    {
        return "flags:bannercontainableboat";
    }
}
