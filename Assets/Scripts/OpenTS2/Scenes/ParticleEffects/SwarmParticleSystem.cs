using System;
using System.Collections.Generic;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Scenes.ParticleEffects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SwarmParticleSystem : MonoBehaviour
    {

        public void SetVisualEffect(SwarmVisualEffect visualEffect, EffectsAsset effectsAsset)
        {
            // Disable the particle system that belongs to this component. We only use it to play all the child particle
            // systems.
            var thisModule = GetComponent<ParticleSystem>().emission;
            thisModule.enabled = false;

            foreach (var effectDescription in visualEffect.Descriptions)
            {
                var effect = effectsAsset.GetEffectFromVisualEffectDescription(effectDescription);
                switch (effect)
                {
                    case ParticleEffect e:
                        var particleEffectSystem = CreateChildSystemForParticleEffect(effectDescription, e);
                        particleEffectSystem.transform.SetParent(transform, worldPositionStays: false);
                        break;
                    case ModelEffect e:
                        var model = ContentProvider.Get()
                            .GetAsset<ScenegraphResourceAsset>(new ResourceKey(e.ModelName, GroupIDs.Scenegraph,
                                TypeIDs.SCENEGRAPH_CRES));
                        var modelObject = model.CreateGameObject();
                        modelObject.transform.SetParent(transform, worldPositionStays: false);
                        break;
                    case DecalEffect e:
                        var decalObject = e.CreateGameObject();
                        decalObject.transform.SetParent(transform, worldPositionStays: false);
                        break;
                    default:
                        throw new NotImplementedException($"Effect type {effect} not supported");
                }
            }
        }

        public void PlayEffect()
        {
            GetComponent<ParticleSystem>().Play(withChildren: true);
        }

        private readonly List<Material> _particleMaterials = new List<Material>();

        // Clean up the materials we created.
        private void OnDestroy()
        {
            foreach (var mat in _particleMaterials)
            {
                mat.Free();
            }
        }

        private GameObject CreateChildSystemForParticleEffect(EffectDescription description, ParticleEffect effect)
        {
            var particleObject = new GameObject(description.EffectName, typeof(ParticleSystem));
            var system = particleObject.GetComponent<ParticleSystem>();
            system.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var emission = system.emission;
            emission.rateOverTime = effect.Emission.RateCurve.ToUnityCurve();

            // Set the emitter shape and direction.
            var shape = system.shape;
            var (emitterPos, emitterRotation, emitterScale) = effect.Emission.EmitVolume.GetCenterRotationAndScale();
            shape.position = emitterPos;
            shape.rotation = emitterRotation.eulerAngles;
            shape.scale = emitterScale;
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

            // Set particle lifetime and speed.
            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(min: effect.Life.Life[0], max: effect.Life.Life[1]);
            main.startSpeed =
                new ParticleSystem.MinMaxCurve(min: effect.Emission.EmitSpeed[0], max: effect.Emission.EmitSpeed[1]);

            // Particle direction.
            var velocityOverTime = system.velocityOverLifetime;
            velocityOverTime.enabled = true;
            velocityOverTime.x = new ParticleSystem.MinMaxCurve(effect.Emission.EmitDirection.LowerCorner[0],
                effect.Emission.EmitDirection.UpperCorner[0]);
            velocityOverTime.y = new ParticleSystem.MinMaxCurve(effect.Emission.EmitDirection.LowerCorner[1],
                effect.Emission.EmitDirection.UpperCorner[1]);
            velocityOverTime.z = new ParticleSystem.MinMaxCurve(effect.Emission.EmitDirection.LowerCorner[2],
                effect.Emission.EmitDirection.UpperCorner[2]);

            // Set particle size over time.
            var sizeOverTime = system.sizeOverLifetime;
            sizeOverTime.size = effect.Size.SizeCurve.ToUnityCurveWithVariance(effect.Size.SizeVary);
            sizeOverTime.enabled = true;

            // Set colors.
            var colorOverLifetime = system.colorOverLifetime;
            var (minColorGradient, maxColorGradient) = effect.Color.GetColorGradientsOverTime();
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(minColorGradient, maxColorGradient);
            colorOverLifetime.enabled = true;

            if (effect.Drawing.MaterialName != "")
            {
                var textureAsset = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey(
                    $"{effect.Drawing.MaterialName}_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR));

                var material = MakeParticleMaterial(textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get()),
                    effect.Drawing.ParticleDrawType);
                _particleMaterials.Add(material);
                system.GetComponent<Renderer>().sharedMaterial = material;
            }

            return particleObject;
        }

        private static Material MakeParticleMaterial(Texture texture, ParticleDrawing.DrawType drawType)
        {
            var material = new Material(Shader.Find("Particles/Standard Unlit"))
            {
                mainTexture = texture
            };
            switch (drawType)
            {
                // These are jacked from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardParticlesShaderGUI.cs#L637
                // for now, may want to do our own shader in the future.

                case ParticleDrawing.DrawType.Decal:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetFloat("_BlendOp", (float)UnityEngine.Rendering.BlendOp.Add);
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0.0f);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                    // Multiplicative color.
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    break;

                case ParticleDrawing.DrawType.Additive:
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

                    // Additive color.
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_ColorAddSubDiff", new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                    break;

                default:
                    throw new NotImplementedException($"Can't render particles with drawType: {drawType}");
            }

            return material;
        }
    }
}