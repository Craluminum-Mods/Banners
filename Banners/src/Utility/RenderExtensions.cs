using Vintagestory.API.Client;

namespace Banners;

public static class RenderExtensions
{
    public static MeshData GenEmptyMesh() => new MeshData(32, 32).WithXyzFaces().WithRenderpasses().WithColorMaps();
}