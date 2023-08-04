using System;
using System.Collections.Generic;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Scenes.ParticleEffects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SwarmMetaParticleSystem : MonoBehaviour
    {
        private ScenegraphResourceAsset _model;
        private ParticleSystem _particleSystem;

        public void SetModelBaseEffect(MetaParticle effect, ModelEffect baseModelEffect)
        {
            _particleSystem = GetComponent<ParticleSystem>();
            // TODO: change this, it's just for testing.
            var main = _particleSystem.main;
            main.maxParticles = 1;

            // Kinda hacky but for now we just render out the scenegraph and get the first mesh and material out of it.
            // Unity's built-in particle system can't spawn full GameObjects but it can handle meshes.
            _model = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(new ResourceKey(baseModelEffect.ModelName,
                GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));
            var modelObject = _model.CreateRootGameObject();
            modelObject.SetActive(false);

            var modelMeshRenderer = modelObject.GetComponentsInChildren<MeshRenderer>();
            Debug.Assert(modelMeshRenderer.Length == 1);
            var modelMeshFilters = modelObject.GetComponentsInChildren<MeshFilter>();
            Debug.Assert(modelMeshFilters.Length == 1);

            var particleRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Mesh;
            particleRenderer.mesh = modelMeshFilters[0].sharedMesh;
            particleRenderer.material = modelMeshRenderer[0].sharedMaterial;
            particleRenderer.alignment = ParticleSystemRenderSpace.Local;
        }
    }
}