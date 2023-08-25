using OpenTS2.Content.DBPF.Scenegraph;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    public class BaseTextureMaterial : StandardMaterial
    {
        public override string Name => "BaseTextureMaterial";

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            return base.Parse(definition);
        }
    }
}