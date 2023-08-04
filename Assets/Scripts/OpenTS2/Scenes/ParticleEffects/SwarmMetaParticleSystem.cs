using System;
using System.Collections.Generic;
using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Scenes.ParticleEffects
{
    [RequireComponent(typeof(ParticleSystem), typeof(AssetReferenceComponent))]
    public class SwarmMetaParticleSystem : MonoBehaviour
    {
        private ScenegraphResourceAsset _model;
        private ParticleSystem _particleSystem;

        private void SetMetaEffectParameters(MetaParticle effect)
        {
            SwarmParticleSystem.SetParticleEmitterRateAndShape(_particleSystem.emission, _particleSystem.shape, effect.Emission, effect.Flags);
            SwarmParticleSystem.SetParticleSpeedAndLifetime(_particleSystem.main, effect.Emission, effect.Life);
            SwarmParticleSystem.SetParticleDirection(_particleSystem.velocityOverLifetime, effect.Emission);
            SwarmParticleSystem.SetParticleSizeOverTime(_particleSystem.sizeOverLifetime, effect.Size);
            SwarmParticleSystem.SetParticleColorOverTime(_particleSystem.colorOverLifetime, effect.Color);
        }

        public void SetModelBaseEffect(MetaParticle effect, ModelEffect baseModelEffect)
        {
            _particleSystem = GetComponent<ParticleSystem>();

            // Set particle system parameters.
            SetMetaEffectParameters(effect);

            // Kinda hacky but for now we just render out the scenegraph and get the first mesh and material out of it.
            // Unity's built-in particle system can't spawn full GameObjects but it can handle meshes.
            _model = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(new ResourceKey(baseModelEffect.ModelName,
                GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));
            var modelObject = _model.CreateRootGameObject();
            modelObject.SetActive(false);

            var modelMeshRenderer = modelObject.GetComponentsInChildren<MeshRenderer>();
            Debug.Assert(modelMeshRenderer.Length == 1, "Model in meta-particle has more than 1 mesh");
            var modelMeshFilters = modelObject.GetComponentsInChildren<MeshFilter>();

            var particleRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Mesh;
            particleRenderer.mesh = modelMeshFilters[0].sharedMesh;
            particleRenderer.material = modelMeshRenderer[0].sharedMaterial;
            particleRenderer.alignment = ParticleSystemRenderSpace.Local;
        }

        public void SetParticleBaseEffect(MetaParticle effect, ParticleEffect baseEffect)
        {
            _particleSystem = GetComponent<ParticleSystem>();

            SwarmParticleSystem.SetParticleParameters(_particleSystem, gameObject, baseEffect);
            SetMetaEffectParameters(effect);
        }
    }
}