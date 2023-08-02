using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common;
using UnityEngine;

namespace OpenTS2.Content
{
    public abstract class TerrainType
    {
        public abstract string Name { get; }
        public virtual float RoadDistanceForRoughness => 40f;
        public virtual float RoughnessFalloff => 40f;
        public virtual Shader TerrainShader => Shader.Find("OpenTS2/ClassicTerrain");
        public virtual bool MakeVariation => true;
        public abstract ResourceKey Texture { get; }
        public virtual ResourceKey Texture1 => Texture;
        public virtual ResourceKey Texture2 => Texture1;
        public abstract ResourceKey Roughness { get; }
        public virtual ResourceKey Roughness1 => Roughness;
        public virtual ResourceKey Roughness2 => Roughness1;
    }
}
