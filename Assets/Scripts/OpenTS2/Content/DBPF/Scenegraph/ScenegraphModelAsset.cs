using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphModelAsset : AbstractAsset
    {
        public Mesh StaticBoundMesh { get; }

        public ScenegraphModelAsset(GeometryDataContainerBlock geometryBlock)
        {
            // Initialize the mesh.
            StaticBoundMesh = new Mesh
            {
                name = geometryBlock.Resource.ResourceName + "_static_bound"
            };
            StaticBoundMesh.SetVertices(geometryBlock.StaticBounds.Vertices);
            StaticBoundMesh.SetTriangles(geometryBlock.StaticBounds.Faces, 0);
        }
    }
}