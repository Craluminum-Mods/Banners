using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Flags;

public class CollectibleBehaviorRenameTool : CollectibleBehavior
{
    public string FromAttribute { get; protected set; } = attributeTitle;

    public CollectibleBehaviorRenameTool(CollectibleObject obj) : base(obj) { }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        if (properties.KeyExists(attributeFromAttribute))
        {
            FromAttribute = properties[attributeFromAttribute].AsString(attributeTitle);
        }
    }
}