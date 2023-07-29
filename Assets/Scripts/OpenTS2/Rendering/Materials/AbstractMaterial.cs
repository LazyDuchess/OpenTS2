using OpenTS2.Content.DBPF.Scenegraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    public abstract class AbstractMaterial
    {
        public abstract string Name { get; }
        public abstract Material Parse(ScenegraphMaterialDefinitionAsset definition);
    }
}
