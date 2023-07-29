using System;
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

        /// <summary>
        /// Gets the model that this GMND points to. Optionally takes a groupId for if the GMND uses a local group id
        /// as its resource key.
        /// </summary>
        public ScenegraphModelAsset GetModelAsset(uint groupId = 0)
        {
            Debug.Assert(GeometryDataContainerKey.TypeID == TypeIDs.SCENEGRAPH_GMDC);
            if (groupId == 0 && GeometryDataContainerKey.GroupID == GroupIDs.Local)
            {
                throw new ArgumentException(
                    "Tried to get a model from a GMND with a Local GroupId but didn't pass groupId");
            }

            var key = GeometryDataContainerKey;
            if (groupId != 0)
                key = key.WithGroupID(groupId);

            var model = ContentProvider.Get().GetAsset<ScenegraphModelAsset>(key);
            Debug.Assert(model != null);
            return model;
        }

        public ScenegraphGeometryNodeAsset(ResourceKey geometryDataContainerKey) =>
            (GeometryDataContainerKey) = (geometryDataContainerKey);
    }
}