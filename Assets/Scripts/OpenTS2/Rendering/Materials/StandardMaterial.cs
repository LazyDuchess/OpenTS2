using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Engine;
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

        /// <summary>
        /// Textures are resolved by name alone in the real engine, independent of the group of the
        /// material referencing them. Shared/CAS textures commonly live in a different package/group than
        /// the material that references them. Try the referencing material's own group first since
        /// that covers the common case cheaply, then fall back to a group-agnostic search.
        /// </summary>
        private static ScenegraphTextureAsset GetTextureAssetByName(string name, uint referencingGroupID)
        {
            var key = new ResourceKey(name + "_txtr", referencingGroupID, TypeIDs.SCENEGRAPH_TXTR);
            var texture = ContentManager.Instance.GetAsset<ScenegraphTextureAsset>(key);
            return texture ?? ContentManager.Instance.GetEntryIgnoringGroup(key)?.GetAsset<ScenegraphTextureAsset>();
        }

        /// <summary>
        /// Composites "baseTexture0".."baseTexture{N-1}" into one texture by alpha-blending each layer
        /// over the previous one using the layer's own alpha channel.
        /// A base skin texture (e.g. "amface-s2", painted with transparent/cutout regions for eyes/eyebrows) has
        /// genetics overlays (e.g. "uuface-eye-ltblue", "uuface-browtweased-red") blended on top.
        /// </summary>
        private static Texture2D GetCompositedBaseTexture(ScenegraphMaterialDefinitionAsset definition, int numTexturesToComposite)
        {
            // TODO: there may be a way to speed this up with Graphics.Blit to offload this to the GPU.
            var compositeName = definition.GetProperty("compositeBaseTextureName", defaultValue: null);

            Texture2D result = null;
            for (var i = 0; i < numTexturesToComposite; i++)
            {
                var layerName = definition.GetProperty($"baseTexture{i}", defaultValue: null);
                if (layerName == null)
                {
                    continue;
                }

                var layerAsset = GetTextureAssetByName(layerName, definition.GlobalTGI.GroupID);
                if (layerAsset == null)
                {
                    Debug.LogWarning($"Unable to find composite layer texture with name: {layerName}");
                    continue;
                }

                var layerTexture = layerAsset.GetSelectedImageAsUnityTexture();
                if (result == null)
                {
                    // The base layer - copied rather than used directly since it's a shared, cached
                    // asset we shouldn't mutate.
                    result = new Texture2D(layerTexture.width, layerTexture.height, TextureFormat.RGBA32, mipChain: false);
                    result.SetPixels32(layerTexture.GetPixels32());
                    continue;
                }

                if (layerTexture.width != result.width || layerTexture.height != result.height)
                {
                    Debug.LogWarning($"Composite layer '{layerName}' is {layerTexture.width}x{layerTexture.height}, " +
                        $"expected {result.width}x{result.height} - skipping");
                    continue;
                }

                var basePixels = result.GetPixels32();
                var overlayPixels = layerTexture.GetPixels32();
                var maxOverlayAlpha = 0;
                for (var p = 0; p < basePixels.Length; p++)
                {
                    if (overlayPixels[p].a > maxOverlayAlpha)
                    {
                        maxOverlayAlpha = overlayPixels[p].a;
                    }
                    var blended = Color32.Lerp(basePixels[p], overlayPixels[p], overlayPixels[p].a / 255f);
                    basePixels[p] = blended;
                }
                result.SetPixels32(basePixels);
            }

            result?.Apply();
            return result;
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
                        bumpMapTexture = GetTextureAssetByName(property.Value, definition.GlobalTGI.GroupID);
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
                        // Genetics (eye/eyebrow color, etc.) come from compositing multiple named
                        // textures together rather than a plain single base texture - see
                        // GetCompositedBaseTexture and SimGeometry.md.
                        var numToComposite = definition.GetProperty("numTexturesToComposite", defaultValue: "1");
                        if (int.Parse(numToComposite) > 1)
                        {
                            var compositedTexture = GetCompositedBaseTexture(definition, int.Parse(numToComposite));
                            if (compositedTexture != null)
                            {
                                material.mainTexture = compositedTexture;
                                break;
                            }
                            Debug.LogWarning($"Unable to composite base texture for: {textureName} - falling back to base layer alone");
                        }
                        var texture = GetTextureAssetByName(textureName, definition.GlobalTGI.GroupID);
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
