using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.ParticleEffects;
using UnityEngine;

namespace OpenTS2.Content
{
    public class EffectsManager
    {
        private static EffectsManager _instance;

        public static EffectsManager Get()
        {
            return _instance;
        }

        public EffectsManager(ContentProvider provider)
        {
            _instance = this;
            _provider = provider;
        }

        private EffectsAsset _effects;
        private readonly ContentProvider _provider;

        public bool Ready { get; private set; }

        public void Initialize()
        {
            // Load effects package.
            _provider.AddPackages(
                Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) +
                                                  "/Res/Effects"));
            _effects = _provider.GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects,
                typeID: TypeIDs.EFFECTS));

            Debug.Assert(_effects != null, "Couldn't find effects");
            Ready = true;
        }

        public bool HasEffect(string effectName)
        {
            return _effects.EffectNamesToIds.ContainsKey(effectName);
        }

        public SwarmParticleSystem CreateEffect(string effectName)
        {
            var visualEffect = _effects.GetEffectByName(effectName);

            var system = new GameObject("SwarmParticleSystem", typeof(ParticleSystem), typeof(SwarmParticleSystem));
            var swarmSystem = system.GetComponent<SwarmParticleSystem>();
            swarmSystem.SetVisualEffect(visualEffect, _effects);

            return swarmSystem;
        }

        public SwarmParticleSystem CreateEffectWithUnityTransform(string effectName)
        {
            var system = CreateEffect(effectName);
            // Perform a sims-space to unity-space transform for this effects system.
            var transform = system.transform;
            transform.Rotate(-90, 0, 0);
            transform.localScale = new Vector3(1, -1, 1);
            return system;
        }
    }
}