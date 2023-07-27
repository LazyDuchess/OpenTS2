using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common;

namespace OpenTS2.Content
{
    public abstract class TerrainType
    {
        public abstract string Name { get; }
        public abstract ResourceKey Texture { get; }
        public virtual ResourceKey Texture1 => Texture;
        public virtual ResourceKey Texture2 => Texture1;
        public abstract ResourceKey Roughness { get; }
        public virtual ResourceKey Roughness1 => Roughness;
        public virtual ResourceKey Roughness2 => Roughness1;
    }
}
