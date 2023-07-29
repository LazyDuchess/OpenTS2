using System;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphShapeAsset : AbstractAsset
    {
        public ShapeBlock ShapeBlock { get; }

        public ScenegraphShapeAsset(ShapeBlock shapeBlock) => (ShapeBlock) = shapeBlock;


        private ScenegraphModelAsset[] _models;
        public ScenegraphModelAsset[] Models
        {
            get
            {
                if (_models == null)
                    throw new InvalidOperationException("Models not loaded, call LoadModelsAndMaterials first");
                return _models;
            }
        }

        private Dictionary<string, ScenegraphMaterialDefinitionAsset> _materials;
        public Dictionary<string, ScenegraphMaterialDefinitionAsset> Materials
        {
            get
            {
                if (_materials == null)
                    throw new InvalidOperationException("Materials not loaded, call LoadModelsAndMaterials first");
                return _materials;
            }
        }


        /// <summary>
        /// Loads the meshes in this shape and the materials needed to render it. Note that this does not bind the
        /// materials to the meshes in `Models`. See `ScenegraphResourceAsset` for a function to fully create a
        /// Unity GameObject that renders the shape with materials and transformations set.
        /// </summary>
        public void LoadModelsAndMaterials()
        {
            // For now we just retrieve the highest LoD geometry.
            var meshes = ShapeBlock.MeshesPerLod[0];
            _models = new ScenegraphModelAsset[meshes.Count];
            for (var i = 0; i < meshes.Count; i++)
            {
                var resourceKey = ResourceKey.ScenegraphResourceKey(meshes[i], GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_GMND);
                var geometryNode = ContentProvider.Get().GetAsset<ScenegraphGeometryNodeAsset>(resourceKey);
                Debug.Assert(geometryNode != null, $"Got null geometry for mesh: {meshes[i]}, key: {resourceKey}");
                _models[i] = geometryNode.GetModelAsset(GlobalTGI.GroupID);
            }

            // Load each material requested by the shape.
            _materials = new Dictionary<string, ScenegraphMaterialDefinitionAsset>();
            foreach (var materialPair in ShapeBlock.Materials)
            {
                var materialKey = ResourceKey.ScenegraphResourceKey($"{materialPair.Value}_txmt", GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_TXMT);
                var material = ContentProvider.Get().GetAsset<ScenegraphMaterialDefinitionAsset>(materialKey);
                _materials[materialPair.Key] = material;
            }
        }
    }
}