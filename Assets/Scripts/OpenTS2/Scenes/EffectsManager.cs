using System;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Scenes
{
    [RequireComponent(typeof(ParticleSystem))]
    public class EffectsManager : MonoBehaviour
    {
        private EffectsAsset _effects;
        private List<Material> _particleMaterials = new List<Material>();

        private void Awake()
        {
            var contentProvider = ContentProvider.Get();
            // Load effects package.
            contentProvider.AddPackages(
                Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) + "/Res/Effects"));
            _effects = contentProvider.GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects, typeID: TypeIDs.EFFECTS));

            Debug.Assert(_effects != null, "Couldn't find effects");

            // Apply a sims to unity space transformation.
            GetComponent<ParticleSystem>().transform.Rotate(-90, 0, 0);
            GetComponent<ParticleSystem>().transform.localScale = new Vector3(1, -1, 1);
            // Disable emission on the base particle system, only children will emit.
            var emissionModule = GetComponent<ParticleSystem>().emission;
            emissionModule.enabled = false;
        }

        // Clean up the materials we created.
        private void OnDestroy()
        {
            foreach(var mat in _particleMaterials)
            {
                mat.Free();
            }
        }

        public void StartEffect(string effectName)
        {
            var unityParticleSystem = GetComponent<ParticleSystem>();

            var visualEffect = _effects.GetEffectByName(effectName);
            foreach (var effectDescription in visualEffect.Descriptions)
            {
                var effect = _effects.GetEffectFromVisualEffectDescription(effectDescription);
                switch (effect)
                {
                    case ParticleEffect e:
                        var subSystem = CreateForParticleEffect(effectDescription, e);
                        subSystem.transform.SetParent(unityParticleSystem.transform, worldPositionStays:false);
                        break;
                    default:
                        throw new NotImplementedException($"Effect type {effect} not supported");
                }
            }

            unityParticleSystem.Play(withChildren:true);
        }

        private GameObject CreateForParticleEffect(EffectDescription description, ParticleEffect effect)
        {
            var particleObject = new GameObject(description.EffectName, typeof(ParticleSystem));
            var system = particleObject.GetComponent<ParticleSystem>();
            system.Stop(withChildren:true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var emission = system.emission;
            emission.rateOverTime = effect.Emission.RateCurve.ToUnityCurve();

            // Set the emitter shape and drection.
            var shape = system.shape;
            if (effect.Emission.EmitTorusWidth > 0)
            {
                shape.shapeType = ParticleSystemShapeType.Donut;
            }
            else if (effect.IsFlagSet(ParticleFlagBits.EmitterIsEllipsoid))
            {
                shape.shapeType = ParticleSystemShapeType.Sphere;
            }
            else
            {
                shape.shapeType = ParticleSystemShapeType.Box;
            }

            var main = system.main;
            main.startSize = effect.Size.SizeCurve.ToUnityCurveWithVariance(effect.Size.SizeVary);
            main.startSpeed =
                new ParticleSystem.MinMaxCurve(min: effect.Emission.EmitSpeed[0], max: effect.Emission.EmitSpeed[1]);
            main.startLifetime = new ParticleSystem.MinMaxCurve(min:effect.Life.Life[0], max:effect.Life.Life[1]);

            var (minColor, maxColor) = effect.Color.GetStartingColorRange();
            main.startColor = new ParticleSystem.MinMaxGradient(minColor, maxColor);

            if (effect.Drawing.MaterialName != "")
            {
                var textureAsset = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey(
                    $"{effect.Drawing.MaterialName}_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR));

                var material = MakeParticleMaterial(textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get()));
                _particleMaterials.Add(material);
                system.GetComponent<Renderer>().sharedMaterial = material;
            }
            Debug.Log($"effect idx: {description.EffectIndex}");

            return particleObject;
        }

        private Material MakeParticleMaterial(Texture texture)
        {
            var material = new Material(Shader.Find("Particles/Standard Surface"))
            {
                mainTexture = texture
            };
            // These are jacked from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardParticlesShaderGUI.cs#L637
            // for now, may want to do our own shader in the future.
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_BlendOp", (float)UnityEngine.Rendering.BlendOp.Add);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_ZWrite", 0.0f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return material;
        }
    }
}