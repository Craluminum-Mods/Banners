using Flags.ToolModes.Banner;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Flags;

public class BlockBehaviorBannerToolModes : BlockBehavior
{
    private ICoreAPI api;

    private SkillItem[] _toolModes;
    public List<BannerToolMode> ToolModes { get; protected set; } = new();
    public List<LoadedTexture> CachedTextures => ObjectCacheUtil.GetOrCreate(api, cacheKeyBannerToolModeTextures, () => new List<LoadedTexture>(ToolModes.Count));

    public BlockBehaviorBannerToolModes(Block block) : base(block) { }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        ToolModes = properties[attributeToolModes].AsObject<List<BannerToolMode>>();
    }

    public override void OnLoaded(ICoreAPI api)
    {
        this.api = api;
    }

    public override void OnUnloaded(ICoreAPI api)
    {
        for (int i = 0; i < CachedTextures.Count; i++)
        {
            CachedTextures[i]?.Dispose();
        }
        ObjectCacheUtil.Delete(api, cacheKeyBannerToolModeTextures);
        ToolModes?.Clear();
    }

    public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int index)
    {
        if (slot.Empty || _toolModes == null || _toolModes.Length <= index) return;

        BannerToolMode matching = ToolModes.FirstOrDefault(x => x.Code == _toolModes[index].Code, null);
        if (matching?.Condition.Matches(slot) == true)
        {
            matching.Condition.SetAttribute(slot);
        }
        slot.MarkDirty();
    }

    public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        if (slot.Empty || forPlayer?.Entity.Api is not ICoreClientAPI capi)
        {
            return null;
        }

        SkillItem[] toolModes = BannerToolMode.GetToolModes(capi, slot, ToolModes);

        CachedTextures.AddRange(toolModes.Where(toolMode => toolMode.Texture != null).Select(toolMode => toolMode.Texture));
        return _toolModes = toolModes;
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
        WorldInteraction[] interactions = new WorldInteraction[] {
            new WorldInteraction()
            {
                ActionLangCode = langCodeSetToolmode,
                HotKeyCode = hotkeyToolmodeSelect,
                MouseButton = EnumMouseButton.None
            }
         };

        return ToolModes?.Any() == true ? interactions : Array.Empty<WorldInteraction>();
    }
}