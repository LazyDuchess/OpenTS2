using OpenTS2.Content.DBPF.Scenegraph;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    public class WallpaperPoolMaterial : StandardMaterial
    {
        public override string Name => "WallpaperPool";

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            return base.Parse(definition);
        }
    }
}