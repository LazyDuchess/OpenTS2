using OpenTS2.Content.DBPF.Scenegraph;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    /// <summary>
    /// Unknown.
    /// </summary>
    public class FloorMaterial : StandardMaterial
    {
        public override string Name => "Floor";

        private static readonly int UVScale = Shader.PropertyToID("_UVScale");

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            // Includes floorMaterialScaleU and floorMaterialScaleV, currently included in the StandardMaterial.
            var material = base.Parse(definition);

            float u = float.Parse(definition.GetProperty("floorMaterialScaleU", defaultValue: "1.0"));
            float v = float.Parse(definition.GetProperty("floorMaterialScaleV", defaultValue: "1.0"));

            material.SetVector(UVScale, new Vector4(1 / u, 1 / v, 0, 0));

            return material;
        }
    }
}