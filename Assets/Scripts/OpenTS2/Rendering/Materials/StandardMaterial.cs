using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Rendering.Materials
{
    public class StandardMaterial : AbstractMaterial
    {
        public override string Name => "StandardMaterial";

        private static readonly int AlphaCutOff = Shader.PropertyToID("_AlphaCutOff");
        private static readonly int AlphaMultiplier = Shader.PropertyToID("_AlphaMultiplier");
        private static readonly int SeaLevel = Shader.PropertyToID("_SeaLevel");
        private static readonly int DiffuseCoefficient = Shader.PropertyToID("_DiffuseCoefficient");
        private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");

        protected virtual Shader GetShader(ScenegraphMaterialDefinitionAsset definition)
        {
            // Decide which shader to use based on the alpha blending and alpha testing.
            if (definition.GetProperty("stdMatAlphaTestEnabled", defaultValue: "0") == "1")
            {
                return Shader.Find("OpenTS2/StandardMaterial/AlphaCutOut");
            }
            else if (definition.GetProperty("stdMatAlphaBlendMode", defaultValue: "none") == "blend")
            {
                return Shader.Find("OpenTS2/StandardMaterial/AlphaBlended");
            }
            else
            {
                return Shader.Find("OpenTS2/StandardMaterial/Opaque");
            }
        }

        public override Material Parse(ScenegraphMaterialDefinitionAsset definition)
        {
            Shader shader = GetShader(definition);

            var material = new Material(shader);
            var bumpMapEnabled = false;
            float alphaMul = 1f;
            float untexturedAlpha = 1f;

            ScenegraphTextureAsset bumpMapTexture = null;
            // Adjust the material properties based on the corresponding keys.
            foreach (var property in definition.MaterialDefinition.MaterialProperties)
            {
                switch (property.Key)
                {
                    case "stdMatNormalMapTextureEnabled":
                        if (property.Value == "true")
                            bumpMapEnabled = true;
                        break;
                    case "stdMatNormalMapTextureName":
                        bumpMapTexture = ContentManager.Instance.GetAsset<ScenegraphTextureAsset>(
                            new ResourceKey(property.Value + "_txtr", definition.GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_TXTR)
                        );
                        if (bumpMapTexture == null)
                        {
                            Debug.Log($"Unable to find bump map texture with name: {bumpMapTexture}");
                            break;
                        }
                        break;
                    case "stdMatAlphaRefValue":
                        var alphaCutoffValue = int.Parse(property.Value);
                        material.SetFloat(AlphaCutOff, alphaCutoffValue / 255.0f);
                        break;
                    case "stdMatBaseTextureName":
                        var textureName = property.Value;
                        var texture = ContentManager.Instance.GetAsset<ScenegraphTextureAsset>(
                            new ResourceKey(textureName + "_txtr", definition.GlobalTGI.GroupID, TypeIDs.SCENEGRAPH_TXTR)
                        );
                        if (texture == null)
                        {
                            Debug.Log($"Unable to find texture with name: {textureName}");
                            break;
                        }
                        definition.Textures.Add(texture);
                        material.mainTexture = texture.GetSelectedImageAsUnityTexture();
                        break;
                    case "stdMatAlphaMultiplier":
                        alphaMul = float.Parse(property.Value);
                        break;
                    case "stdMatUntexturedDiffAlpha":
                        untexturedAlpha = float.Parse(property.Value);
                        break;
                    case "stdMatDiffCoef":
                        var coefficients = property.Value.Split(',').Select(float.Parse).ToArray();
                        material.SetColor(DiffuseCoefficient, new Color(coefficients[0], coefficients[1], coefficients[2]));
                        break;
                }
            }

            material.SetFloat(AlphaMultiplier, alphaMul * untexturedAlpha);

            if (bumpMapEnabled && bumpMapTexture != null)
            {
                definition.Textures.Add(bumpMapTexture);
                material.SetTexture(BumpMap, bumpMapTexture.GetSelectedImageAsUnityTexture());
            }

            var neighborhood = NeighborhoodManager.Instance.CurrentNeighborhood;
            if (neighborhood != null)
            {
                material.SetFloat(SeaLevel, NeighborhoodManager.Instance.CurrentNeighborhood.Terrain.SeaLevel);
            }

            return material;
        }
    }
}
