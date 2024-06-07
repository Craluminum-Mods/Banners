using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Flags;

public class CollectibleBehaviorBannerPatternToolModes : CollectibleBehavior
{
    public List<PatternToolMode> ToolModes { get; protected set; } = new();

    public CollectibleBehaviorBannerPatternToolModes(CollectibleObject collObj) : base(collObj) { }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        ToolModes = properties[attributeToolModes].AsObject<List<PatternToolMode>>();
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        ToolModes?.Clear();
    }

    public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int index)
    {
        if (slot.Empty) return;

        if (ToolModes.Count <= index)
        {
            return;
        }

        PatternToolMode.TryUnlockAll(ToolModes, slot, null, skipStack: true);

        ItemSlot mouseslot = byPlayer.InventoryManager.MouseItemSlot;
        if (!mouseslot.Empty)
        {
            PatternToolMode.TryUnlockAll(ToolModes, slot, mouseslot.Itemstack);
            byPlayer.Entity.World.Api.Event.PushEvent(eventKeepOpenToolmodeDialog);
        }
        else if (ToolModes[index].IsUnlocked(slot) || byPlayer.WorldData.CurrentGameMode is EnumGameMode.Creative)
        {
            ToolModes[index].SetPattern(slot.Itemstack);
        }

        slot.MarkDirty();
    }

    public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        if (slot.Empty || forPlayer?.Entity.Api is not ICoreClientAPI capi)
        {
            return null;
        }
        return PatternToolMode.GetToolModes(capi, slot, ToolModes);
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
        WorldInteraction[] interactions = new WorldInteraction[] {
            new WorldInteraction()
            {
                ActionLangCode = langCodeBannerBannerPatternSet,
                HotKeyCode = hotkeyToolmodeSelect,
                MouseButton = EnumMouseButton.None
            }
         };

        return ToolModes?.Any() == true ? interactions : Array.Empty<WorldInteraction>();
    }
}