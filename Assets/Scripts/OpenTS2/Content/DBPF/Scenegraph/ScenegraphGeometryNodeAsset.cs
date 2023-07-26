using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    /// <summary>
    /// A Scenegraph GMND, just a level of indirection to a GMDC.
    /// </summary>
    public class ScenegraphGeometryNodeAsset : AbstractAsset
    {
        public ResourceKey GeometryDataContainerKey { get; }

        public ScenegraphModelAsset GetModelAsset()
        {
            Debug.Assert(GeometryDataContainerKey.TypeID == TypeIDs.SCENEGRAPH_GMDC);
            return ContentProvider.Get().GetAsset<ScenegraphModelAsset>(GeometryDataContainerKey);
        }

        public ScenegraphGeometryNodeAsset(ResourceKey geometryDataContainerKey) =>
            (GeometryDataContainerKey) = (geometryDataContainerKey);
    }
}