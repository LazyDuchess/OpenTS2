using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTS2.Common;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;
using UnityEngine.Rendering;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphMaterialDefinitionAsset : AbstractAsset
    {
        private List<ScenegraphTextureAsset> _textures = new List<ScenegraphTextureAsset>();

        public MaterialDefinitionBlock MaterialDefinition { get; }

        public ScenegraphMaterialDefinitionAsset(MaterialDefinitionBlock material) => (MaterialDefinition) = (material);

        private string GetProperty(string key, string defaultValue = null)
        {
            if (MaterialDefinition.MaterialProperties.TryGetValue(key, out var value))
            {
                return value;
            }

            if (defaultValue == null)
                throw new KeyNotFoundException($"{key} not in material properties and no default given");
            return defaultValue;
        }

        // Stores the cached Material
        private Material _material;

        public override void FreeUnmanagedResources()
        {
            if (_material == null)
                return;
            _material.Free();
        }

        public Material GetAsUnityMaterial()
        {
            if (_material != null)
            {
                return _material;
            }

            var material = MaterialDefinition.Type switch
            {
                MaterialType.StandardMaterial => GetStandardMaterial(),
                MaterialType.ImposterMaterial => GetImposterMaterial(),
                _ => throw new ArgumentOutOfRangeException()
            };
            _material = material;
            return material;
        }

        private static readonly int AlphaCutOff = Shader.PropertyToID("_AlphaCutOff");
        private static readonly int AlphaMultiplier = Shader.PropertyToID("_AlphaMultiplier");
        private static readonly int SeaLevel = Shader.PropertyToID("_SeaLevel");

        private Material GetStandardMaterial()
        {
            Shader shader;
            // Decide which shader to use based on the alpha blending and alpha testing.
            if (GetProperty("stdMatAlphaTestEnabled", defaultValue: "0") == "1")
            {
                shader = Shader.Find("OpenTS2/StandardMaterial/AlphaCutOut");
            }
            else if (GetProperty("stdMatAlphaBlendMode", defaultValue: "none") == "blend")
            {
                shader = Shader.Find("OpenTS2/StandardMaterial/AlphaBlended");
            }
            else
            {
                shader = Shader.Find("OpenTS2/StandardMaterial/Opaque");
            }

            var material = new Material(shader);

            // Adjust the material properties based on the corresponding keys.
            foreach (var property in MaterialDefinition.MaterialProperties)
            {
                switch (property.Key)
                {
                    case "stdMatAlphaRefValue":
                        var alphaCutoffValue = int.Parse(property.Value);
                        material.SetFloat(AlphaCutOff, alphaCutoffValue / 255.0f);
                        break;
                    case "stdMatBaseTextureName":
                        var textureName = property.Value;
                        var texture = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(
                            new ResourceKey(textureName + "_txtr", GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_TXTR)
                        );
                        _textures.Add(texture);
                        material.mainTexture = texture.GetSelectedImageAsUnityTexture(ContentProvider.Get());
                        break;
                    case "stdMatAlphaMultiplier":
                        material.SetFloat(AlphaMultiplier, float.Parse(property.Value));
                        break;
                }
            }

            material.SetFloat(SeaLevel, NeighborhoodManager.CurrentNeighborhood.Terrain.SeaLevel);

            return material;
        }

        private Material GetImposterMaterial()
        {
            // TODO: For now these just use the standard shader, see if we need something different.
            var shader = Shader.Find("OpenTS2/StandardMaterial/AlphaCutOut");
            var material = new Material(shader);

            Debug.Log("imposter material: " + MaterialDefinition);
            string textureName;
            if (MaterialDefinition.MaterialName.Contains("terrainmaterial"))
            {
                textureName = "terrain_txtr";
            }
            else if (MaterialDefinition.MaterialName.Contains("roof"))
            {
                textureName = "roofs_txtr";
            }
            else if (MaterialDefinition.MaterialName.Contains("wall"))
            {
                textureName = $"walls_{GetProperty("page")}_txtr";
            }
            else if (MaterialDefinition.MaterialName.Contains("slicepage"))
            {
                textureName = $"slices_{GetProperty("page")}_txtr";
            }
            else
            {
                throw new NotImplementedException($"Unknown imposter material: {MaterialDefinition.MaterialName}");
            }

            var texture = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(
                new ResourceKey(textureName, GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_TXTR)
            );
            _textures.Add(texture);
            material.mainTexture = texture.GetSelectedImageAsUnityTexture(ContentProvider.Get());

            return material;
        }
    }
}