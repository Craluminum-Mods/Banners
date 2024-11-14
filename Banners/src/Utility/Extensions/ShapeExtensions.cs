using Vintagestory.API.Common;

namespace Flags;

public static class ShapeExtensions
{
    public static Shape RemoveWindData(this Shape shape)
    {
        shape?.WalkElements("*", (ShapeElement shapeElement) =>
        {
            foreach (ShapeElementFace face in shapeElement.FacesResolved)
            {
                if (face == null)
                {
                    continue;
                }
                if (face.WindMode != null)
                {
                    for (int i = 0; i < face.WindMode.Length; i++)
                    {
                        face.WindMode[i] = 0;
                    }
                }
                if (face.WindData != null)
                {
                    for (int i = 0; i < face.WindData.Length; i++)
                    {
                        face.WindData[i] = 0;
                    }
                }
            }
        });

        return shape;
    }

    public static Shape PrefixTextures(this Shape shape, string prefix)
    {
        shape?.WalkElements("*", (ShapeElement shapeElement) =>
        {
            foreach (ShapeElementFace face in shapeElement.FacesResolved)
            {
                if (face == null || string.IsNullOrEmpty(face.Texture))
                {
                    continue;
                }

                face.Texture = prefix + face.Texture;
            }
        });

        return shape;
    }
}