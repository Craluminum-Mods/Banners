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
}
