using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Flags;

public class CollectibleBehaviorBannerPatternDescription : CollectibleBehavior
{
    public CollectibleBehaviorBannerPatternDescription(CollectibleObject obj) : base(obj) { }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (!inSlot.Empty && TryGetProperties(out BannerPatternProperties props, inSlot.Itemstack))
        {
            string descLangCode = $"{langCodePatternDesc}{props.Type}";
            if (Lang.HasTranslation(descLangCode))
            {
                dsc.Append(descLangCode.Localize() + '\n');
            }
        }
    }

    protected static bool TryGetProperties(out BannerPatternProperties props, ItemStack stack)
    {
        props = BannerPatternProperties.FromStack(stack);
        return props != null;
    }
}