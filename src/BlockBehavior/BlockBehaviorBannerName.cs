using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Flags;

public class BlockBehaviorBannerName : BlockBehavior
{
    public List<string> Parts { get; protected set; } = new();
    public string ReplacePart { get; protected set; }
    public string DefaultName { get; protected set; }

    public BlockBehaviorBannerName(Block block) : base(block) { }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);

        Parts = properties[attributeParts].AsObject<List<string>>(new());
        ReplacePart = properties.KeyExists(attributeReplacePart) ? properties[attributeReplacePart].AsString() : "";
        DefaultName = properties.KeyExists(attributeDefaultName) ? properties[attributeDefaultName].AsString() : "";
    }

    public override void GetPlacedBlockName(StringBuilder sb, IWorldAccessor world, BlockPos pos) => ConstructName(sb: sb, world: world, pos: pos);

    public override void GetHeldItemName(StringBuilder sb, ItemStack itemStack) => ConstructName(sb, itemStack);

    public string ConstructName(StringBuilder sb, ItemStack stack = null, IWorldAccessor world = null, BlockPos pos = null)
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
        else if (TryGetProperties(out BannerProperties props, stack, world, pos))
        {
            newName = !string.IsNullOrEmpty(props.Name)
                ? props.Name
                : string.Join("", Parts.Select(x => x.Replace(ReplacePart, props.BaseColor).LocalizeM()));

            sb.Clear();
            sb.Append(newName);
            return sb.ToString();
        }
        return newName;
    }

    protected static bool TryGetProperties(out BannerProperties props, ItemStack stack = null, IWorldAccessor world = null, BlockPos pos = null)
    {
        props = null;

        if (stack != null)
        {
            props = BannerProperties.FromStack(stack);
        }
        else if (world != null && pos != null && world.BlockAccessor.GetBlockEntity(pos) is BlockEntityBanner be)
        {
            props = be.BannerProps;
        }
        return props != null;
    }
}