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


        private ScenegraphModelAsset[] models;
        public ScenegraphModelAsset[] Models
        {
            get
            {
                if (models == null)
                    throw new InvalidOperationException("Models not loaded, call LoadModelsAndMaterials first");
                return models;
            }
        }

        private Dictionary<string, ScenegraphMaterialDefinitionAsset> materials;
        public Dictionary<string, ScenegraphMaterialDefinitionAsset> Materials
        {
            get
            {
                if (materials == null)
                    throw new InvalidOperationException("Materials not loaded, call LoadModelsAndMaterials first");
                return materials;
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
            models = new ScenegraphModelAsset[meshes.Count];
            for (var i = 0; i < meshes.Count; i++)
            {
                var resourceKey = new ResourceKey(meshes[0], GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_GMND);
                var geometryNode = ContentProvider.Get().GetAsset<ScenegraphGeometryNodeAsset>(resourceKey);
                models[i] = geometryNode.GetModelAsset();
            }

            // Load each material requested by the shape.
            materials = new Dictionary<string, ScenegraphMaterialDefinitionAsset>();
            foreach (var materialPair in ShapeBlock.Materials)
            {
                var materialKey = new ResourceKey($"{materialPair.Value}_txmt", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXMT);
                var material = ContentProvider.Get().GetAsset<ScenegraphMaterialDefinitionAsset>(materialKey);
                materials[materialPair.Key] = material;

                /*
                foreach (var model in models)
                {
                    if (!model.Primitives.TryGetValue(materialPair.Key, out var mesh))
                    {
                        continue;
                    }
                }
                */
            }
        }
    }
}