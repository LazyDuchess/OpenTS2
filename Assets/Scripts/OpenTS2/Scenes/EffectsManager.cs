using System;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Scenes
{
    [RequireComponent(typeof(ParticleSystem))]
    public class EffectsManager : MonoBehaviour
    {
        private EffectsAsset _effects;

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

        private static GameObject CreateForParticleEffect(EffectDescription description, ParticleEffect effect)
        {
            var particleObject = new GameObject(description.EffectName, typeof(ParticleSystem));
            var system = particleObject.GetComponent<ParticleSystem>();

            var emission = system.emission;
            var emissionRateOverTime = emission.rateOverTime;
            emissionRateOverTime.curve = effect.Emission.RateCurve.ToUnityCurve();

            return particleObject;
        }
    }
}