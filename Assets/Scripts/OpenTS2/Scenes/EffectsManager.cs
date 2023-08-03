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
using OpenTS2.Scenes.ParticleEffects;
using UnityEngine;

namespace OpenTS2.Scenes
{
    public class EffectsManager : MonoBehaviour
    {
        private EffectsAsset _effects;

        private void Awake()
        {
            var contentProvider = ContentProvider.Get();
            // Load effects package.
            contentProvider.AddPackages(
                Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) +
                                                  "/Res/Effects"));
            _effects = contentProvider.GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects,
                typeID: TypeIDs.EFFECTS));

            Debug.Assert(_effects != null, "Couldn't find effects");
        }

        public SwarmParticleSystem CreateEffect(string effectName)
        {
            var visualEffect = _effects.GetEffectByName(effectName);

            var system = new GameObject("SwarmParticleSystem", typeof(ParticleSystem), typeof(SwarmParticleSystem));
            var swarmSystem = system.GetComponent<SwarmParticleSystem>();
            swarmSystem.SetVisualEffect(visualEffect, _effects);

            return swarmSystem;
        }

    }
}