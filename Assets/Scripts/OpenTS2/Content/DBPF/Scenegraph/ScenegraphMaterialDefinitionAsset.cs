using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTS2.Common;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphMaterialDefinitionAsset : AbstractAsset
    {
        public List<ScenegraphTextureAsset> Textures = new List<ScenegraphTextureAsset>();

        public MaterialDefinitionBlock MaterialDefinition { get; }

        public ScenegraphMaterialDefinitionAsset(MaterialDefinitionBlock material) => (MaterialDefinition) = (material);

        public string GetProperty(string key, string defaultValue = null)
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
            _material = MaterialManager.Parse(this);
            return _material;
        }
    }
}