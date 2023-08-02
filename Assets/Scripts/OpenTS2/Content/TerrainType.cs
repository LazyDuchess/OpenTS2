using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common;
using UnityEngine;

namespace OpenTS2.Content
{
    public class TerrainType
    {
        public string Name;
        public float RoadDistanceForRoughness = 40f;
        public float RoughnessFalloff = 40f;
        public Shader TerrainShader = Shader.Find("OpenTS2/ClassicTerrain");
        public bool MakeVariation = true;
        public ResourceKey Texture;
        public ResourceKey Texture1;
        public ResourceKey Texture2;
        public ResourceKey Roughness;
        public ResourceKey Roughness1;
        public ResourceKey Roughness2;
        public string RoadTextureName = "new_roads_{0}_txtr";
    }
}
