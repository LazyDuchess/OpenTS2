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

        private void Start()
        {
            var contentProvider = ContentProvider.Get();
            // Load effects package.
            contentProvider.AddPackages(
                Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) + "/Res/Effects"));
            _effects = contentProvider.GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects, typeID: TypeIDs.EFFECTS));

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
                        subSystem.transform.parent = unityParticleSystem.transform;
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
            var emissionRateOverTime = emission.rateOverTime;
            emissionRateOverTime.curve = effect.Emission.RateCurve.ToUnityCurve();

            var main = system.main;
            main.duration = effect.Life.Life[0];

            if (effect.Drawing.MaterialName != "")
            {
                var textureAsset = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey(
                    $"{effect.Drawing.MaterialName}_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR));

                // TODO: temporarily using the standard shader here.
                var material = new Material(Shader.Find("OpenTS2/StandardMaterial/AlphaBlended"))
                {
                    mainTexture = textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get())
                };
                _particleMaterials.Add(material);

                system.GetComponent<Renderer>().sharedMaterial = material;

                Debug.Log($"material: {effect.Drawing.MaterialName}");
            }

            return particleObject;
        }
    }
}