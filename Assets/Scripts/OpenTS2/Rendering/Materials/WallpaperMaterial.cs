using OpenTS2.Content.DBPF.Scenegraph;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    public class WallpaperMaterial : StandardMaterial
    {
        public override string Name => "Wallpaper";

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            // TODO: mask

            definition.MaterialDefinition.MaterialProperties["stdMatAlphaTestEnabled"] = "1";
            definition.MaterialDefinition.MaterialProperties["stdMatAlphaRefValue"] = "128";

            return base.Parse(definition);
        }
    }
}