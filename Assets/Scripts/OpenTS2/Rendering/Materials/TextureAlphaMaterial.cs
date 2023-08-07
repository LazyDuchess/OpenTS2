using OpenTS2.Content.DBPF.Scenegraph;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    /// <summary>
    /// This appears to just be a StandardMaterial with alpha fading implicitly enabled.
    /// </summary>
    public class TextureAlphaMaterial : StandardMaterial
    {
        public override string Name => "TextureAlpha";

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            definition.MaterialDefinition.MaterialProperties["stdMatAlphaBlendMode"] = "blend";
            return base.Parse(definition);
        }
    }
}