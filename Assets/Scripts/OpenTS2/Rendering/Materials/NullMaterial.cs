using OpenTS2.Content.DBPF.Scenegraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    public class NullMaterial : AbstractMaterial
    {
        public override string Name => "Null";

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            var shader = Shader.Find("OpenTS2/NullShader");
            return new Material(shader);
        }
    }
}
