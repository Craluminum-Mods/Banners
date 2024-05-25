using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Flags;

public class CollectibleBehaviorBannerPatternName : CollectibleBehavior
{
    public List<string> Parts { get; protected set; } = new();
    public string ReplacePart { get; protected set; }
    public string DefaultName { get; protected set; }

    public CollectibleBehaviorBannerPatternName(CollectibleObject obj) : base(obj) { }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        Parts = properties[attributeParts].AsObject<List<string>>(new());
        ReplacePart = properties.KeyExists(attributeReplacePart) ? properties[attributeReplacePart].AsString() : "";
        DefaultName = properties.KeyExists(attributeDefaultName) ? properties[attributeDefaultName].AsString() : "";
    }

    public override void GetHeldItemName(StringBuilder sb, ItemStack itemStack) => ConstructName(sb, itemStack);

    public string ConstructName(StringBuilder sb, ItemStack stack)
    {
        string newName = DefaultName.LocalizeM();
        if (!Parts.Any()) return newName;
        if (string.IsNullOrEmpty(ReplacePart))
        {
            newName = string.Join("", Parts.Select(x => x.LocalizeM()));
            sb.Clear();
            sb.Append(newName);
            return sb.ToString();
        }
        else if (TryGetProperties(out BannerPatternProperties props, stack))
        {
            newName = string.Join("", Parts.Select(x => x.Replace(ReplacePart, props.Type).LocalizeM()));
            sb.Clear();
            sb.Append(newName);
            return sb.ToString();
        }
        return newName;
    }

    protected static bool TryGetProperties(out BannerPatternProperties props, ItemStack stack)
    {
        props = BannerPatternProperties.FromStack(stack);
        return props != null;
    }
}