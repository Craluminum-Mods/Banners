using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flags;

public class CollectibleBehaviorBannerLiquidDescription : CollectibleBehavior
{
    public CollectibleBehaviorBannerLiquidDescription(CollectibleObject collObj) : base(collObj) { }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder sb, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, sb, world, withDebugInfo);

        if (!BannerLiquid.TryGet(collObj, out BannerLiquid liquidProps)
            && (collObj is not BlockLiquidContainerTopOpened container || !BannerLiquid.TryGet(inSlot.Itemstack, container, out liquidProps)))
        {
            return;
        }

        sb.AppendLine(langCodeBannerLiquidType.Localize($"{modDomain}:{liquidProps.Type}".Localize()));

        if (!string.IsNullOrEmpty(liquidProps.Color))
        {
            sb.AppendLine(langCodeBannerColorType.Localize($"{langCodeColor}{liquidProps.Color}".Localize()));
        }
    }
}