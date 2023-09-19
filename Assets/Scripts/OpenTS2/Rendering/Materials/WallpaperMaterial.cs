using OpenTS2.Content.DBPF.Scenegraph;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    public class WallpaperMaterial : StandardMaterial
    {
        public override string Name => "Wallpaper";

        protected override Shader GetShader(ScenegraphMaterialDefinitionAsset definition)
        {
            return Shader.Find("OpenTS2/StandardMaterial/Wall");
        }

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            // TODO: mask

            definition.MaterialDefinition.MaterialProperties["stdMatAlphaTestEnabled"] = "1";
            definition.MaterialDefinition.MaterialProperties["stdMatAlphaRefValue"] = "128";

            return base.Parse(definition);
        }
    }
}