using Vintagestory.API.MathTools;

namespace Flags;

public class RotationsByFace
{
    public Vec3f North { get; set; } = Vec3f.Zero;
    public Vec3f East { get; set; } = Vec3f.Zero;
    public Vec3f South { get; set; } = Vec3f.Zero;
    public Vec3f West { get; set; } = Vec3f.Zero;
    public Vec3f Up { get; set; } = Vec3f.Zero;
    public Vec3f Down { get; set; } = Vec3f.Zero;

    public Vec3f[] Faces => new Vec3f[6] { North, East, South, West, Up, Down };

    public Vec3f this[BlockFacing facing]
    {
        get => Faces[facing.Index];
        set
        {
            switch (facing.Index)
            {
                case BlockFacing.indexNORTH:
                    North = value;
                    break;
                case BlockFacing.indexEAST:
                    East = value;
                    break;
                case BlockFacing.indexSOUTH:
                    South = value;
                    break;
                case BlockFacing.indexWEST:
                    West = value;
                    break;
                case BlockFacing.indexUP:
                    Up = value;
                    break;
                case BlockFacing.indexDOWN:
                    Down = value;
                    break;
            }
        }
    }

    // public RotationsByFace(Dictionary<string, Vec3f> rotations = null)
    // {
    //     if (rotations == null)
    //     {
    //         return;
    //     }

    //     foreach ((string key, Vec3f val) in rotations)
    //     {
    //         this[BlockFacing.FromCode(key)] = val;
    //     }
    // }

    // /// <summary>
    // /// Only used for debug reasons
    // /// </summary>
    // public RotationsByFace FromTreeAttribute(ITreeAttribute tree)
    // {
    //     ITreeAttribute debugTree = tree.GetOrAddTreeAttribute("debugRotations");
    //     foreach (BlockFacing facing in BlockFacing.ALLFACES)
    //     {
    //         ITreeAttribute faceTree = debugTree.GetOrAddTreeAttribute(facing.Code);
    //         Vec3f rotation = new Vec3f(faceTree.GetFloat("x"), faceTree.GetFloat("y"), faceTree.GetFloat("z"));
    //         this[facing] = rotation;
    //     }
    //     return this;
    // }

    // /// <summary>
    // /// Only used for debug reasons
    // /// </summary>
    // public void ToTreeAttribute(ITreeAttribute tree)
    // {
    //     ITreeAttribute debugTree = tree.GetOrAddTreeAttribute("debugRotations");
    //     foreach (BlockFacing facing in BlockFacing.ALLFACES)
    //     {
    //         ITreeAttribute faceTree = debugTree.GetOrAddTreeAttribute(facing.Code);
    //         Vec3f rotation = this[facing];
    //         faceTree.SetFloat("x", rotation.X);
    //         faceTree.SetFloat("y", rotation.Y);
    //         faceTree.SetFloat("z", rotation.Z);
    //     }
    // }
}
