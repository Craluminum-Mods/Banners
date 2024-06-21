using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;

namespace Flags;

public class CollectibleBehaviorBannerPatternDescription : CollectibleBehavior
{
    public CollectibleBehaviorBannerPatternDescription(CollectibleObject obj) : base(obj) { }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        if (!inSlot.Empty && TryGetProperties(out PatternProperties props, inSlot.Itemstack))
        {
            string descLangCode = $"{langCodePatternDesc}{props.Type}";
            if (descLangCode.HasTranslation())
            {
                dsc.Append(descLangCode.Localize() + '\n');
            }
            IEnumerable<string> unlockedTypes = props.UnlockedTypes.Where(x => !string.IsNullOrEmpty(x));
            if (unlockedTypes.Any())
            {
                string types = string.Join(commaSeparator, unlockedTypes.Select(type => $"{langCodePattern}{type}".Localize()));
                dsc.AppendLine(langCodeUnlockedPatterns.Localize(types));
            }
        }
    }

    protected static bool TryGetProperties(out PatternProperties props, ItemStack stack)
    {
        props = PatternProperties.FromStack(stack);
        return props != null;
    }
}