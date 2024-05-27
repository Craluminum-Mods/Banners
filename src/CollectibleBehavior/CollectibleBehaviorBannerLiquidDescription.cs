using System.Text;
using Vintagestory.API.Common;

namespace Flags;

public class CollectibleBehaviorBannerLiquidDescription : CollectibleBehavior
{
    public CollectibleBehaviorBannerLiquidDescription(CollectibleObject collObj) : base(collObj) { }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder sb, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, sb, world, withDebugInfo);

        if (!BannerLiquid.TryGet(collObj, out BannerLiquid liquidProps))
        {
            return;
        }

        sb.AppendLine(langCodeBannerLiquidType.Localize(liquidProps.Type.ToString().Localize()));

        if (!string.IsNullOrEmpty(liquidProps.Color))
        {
            sb.AppendLine(langCodeBannerColorType.Localize($"{langCodeColor}{liquidProps.Color}".Localize()));
        }
    }
}