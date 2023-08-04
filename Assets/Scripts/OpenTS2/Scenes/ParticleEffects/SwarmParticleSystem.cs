using System;
using System.Collections.Generic;
using OpenTS2.Common;
using OpenTS2.Components;
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
                    case SwarmVisualEffect e:
                        // TODO: there is a selectionChance here that needs to be considered.
                        var system = new GameObject("SwarmParticleSystem", typeof(ParticleSystem), typeof(SwarmParticleSystem));
                        system.GetComponent<SwarmParticleSystem>().SetVisualEffect(e, effectsAsset);
                        system.transform.SetParent(transform, worldPositionStays: false);
                        break;
                    case ParticleEffect e:
                        var particleEffectSystem = CreateChildSystemForParticleEffect(effectDescription, e);
                        particleEffectSystem.transform.SetParent(transform, worldPositionStays: false);
                        break;
                    case MetaParticle e:
                        var metaParticleSystem =
                            CreateGameObjectForMetaParticleSystem(effectDescription.EffectName, e, effectsAsset);
                        metaParticleSystem.transform.SetParent(transform, worldPositionStays: false);
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

        private static GameObject CreateGameObjectForMetaParticleSystem(string effectName, MetaParticle e,
            EffectsAsset effectsAsset)
        {
            // A meta-particle is basically "extra-stuff" like an altered lifetime or emission rate attached
            // to an existing particle effect. For now we just handle some particular types of child effects.
            var metaParticles = new GameObject(effectName);

            var baseVisualEffect = effectsAsset.GetEffectByName(e.BaseEffect);
            foreach (var desc in baseVisualEffect.Descriptions)
            {
                var baseEffect =
                    effectsAsset.GetEffectFromVisualEffectDescription(desc);

                var particleObject = new GameObject(desc.EffectName, typeof(SwarmMetaParticleSystem));
                var metaSystem = particleObject.GetComponent<SwarmMetaParticleSystem>();

                switch (baseEffect)
                {
                    case ModelEffect modelEffect:
                        metaSystem.SetModelBaseEffect(e, modelEffect);
                        break;
                    case ParticleEffect particleEffect:
                        metaSystem.SetParticleBaseEffect(e, particleEffect);
                        break;
                    default:
                        throw new NotImplementedException(
                            $"Can't handle meta-particles with a base effect of {baseEffect}");
                }

                particleObject.transform.SetParent(metaParticles.transform, worldPositionStays: false);
            }

            return metaParticles;
        }

        private static GameObject CreateChildSystemForParticleEffect(EffectDescription description,
            ParticleEffect effect)
        {
            var particleObject = new GameObject(description.EffectName, typeof(ParticleSystem),
                typeof(AssetReferenceComponent));
            var system = particleObject.GetComponent<ParticleSystem>();
            system.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);

            SetParticleParameters(system, particleObject, effect);

            return particleObject;
        }

        internal static void SetParticleParameters(ParticleSystem system, GameObject particleObject, ParticleEffect effect)
        {
            SetParticleEmitterRateAndShape(system.emission, system.shape, effect.Emission, effect.Flags);
            SetParticleSpeedAndLifetime(system.main, effect.Emission, effect.Life);
            SetParticleDirection(system.velocityOverLifetime, effect.Emission);
            SetParticleSizeOverTime(system.sizeOverLifetime, effect.Size);
            SetParticleColorOverTime(system.colorOverLifetime, effect.Color);

            if (effect.Drawing.MaterialName != "")
            {
                var textureAsset = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey(
                    $"{effect.Drawing.MaterialName}_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR));

                var material = MakeParticleMaterial(textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get()),
                    effect.Drawing.ParticleDrawType);
                system.GetComponent<Renderer>().sharedMaterial = material;
                particleObject.GetComponent<AssetReferenceComponent>().AddReference(textureAsset);
            }
        }

        internal static void SetParticleEmitterRateAndShape(ParticleSystem.EmissionModule emissionModule,
            ParticleSystem.ShapeModule shape, ParticleEmission emission, ulong flags)
        {
            // Set emitter rate.
            emissionModule.rateOverTime = emission.RateCurve.ToUnityCurve();

            // Set the emitter shape.
            var (emitterPos, emitterRotation, emitterScale) = emission.EmitVolume.GetCenterRotationAndScale();
            shape.position = emitterPos;
            shape.rotation = emitterRotation.eulerAngles;
            shape.scale = emitterScale;
            if (emission.EmitTorusWidth > 0)
            {
                shape.shapeType = ParticleSystemShapeType.Donut;
            }
            else if (ParticleFlagBits.EmitterIsEllipsoid.IsFlagSet(flags))
            {
                shape.shapeType = ParticleSystemShapeType.Sphere;
            }
            else
            {
                shape.shapeType = ParticleSystemShapeType.Box;
            }
        }

        internal static void SetParticleSpeedAndLifetime(ParticleSystem.MainModule main, ParticleEmission emission, ParticleLife life)
        {
            main.startLifetime = new ParticleSystem.MinMaxCurve(min: life.Life[0], max: life.Life[1]);
            main.startSpeed =
                new ParticleSystem.MinMaxCurve(min: emission.EmitSpeed[0], max: emission.EmitSpeed[1]);
        }

        internal static void SetParticleDirection(ParticleSystem.VelocityOverLifetimeModule velocityOverTime,
            ParticleEmission emission)
        {
            velocityOverTime.enabled = true;
            velocityOverTime.x = new ParticleSystem.MinMaxCurve(emission.EmitDirection.LowerCorner[0],
                emission.EmitDirection.UpperCorner[0]);
            velocityOverTime.y = new ParticleSystem.MinMaxCurve(emission.EmitDirection.LowerCorner[1],
                emission.EmitDirection.UpperCorner[1]);
            velocityOverTime.z = new ParticleSystem.MinMaxCurve(emission.EmitDirection.LowerCorner[2],
                emission.EmitDirection.UpperCorner[2]);
        }

        internal static void SetParticleSizeOverTime(ParticleSystem.SizeOverLifetimeModule sizeOverTime, ParticleSize size)
        {
            sizeOverTime.size = size.SizeCurve.ToUnityCurveWithVariance(size.SizeVary);
            sizeOverTime.enabled = true;
        }

        internal static void SetParticleColorOverTime(ParticleSystem.ColorOverLifetimeModule colorOverLifetime,
            ParticleColor color)
        {
            var (minColorGradient, maxColorGradient) = color.GetColorGradientsOverTime();
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(minColorGradient, maxColorGradient);
            colorOverLifetime.enabled = true;
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