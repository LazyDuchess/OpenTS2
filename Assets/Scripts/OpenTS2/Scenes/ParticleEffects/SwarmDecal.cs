﻿using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Diagnostic;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Scenes.ParticleEffects
{
    public class SwarmDecal : MonoBehaviour
    {
        private static int Location = Shader.PropertyToID("_Location");
        private static int Rotation = Shader.PropertyToID("_Rotation");
        private ScenegraphTextureAsset _textureAsset;
        private Vector3 _position;
        private Vector3 _rotation;
        private Material _material;

        private void Start()
        {
            _position = transform.position;
            _rotation = transform.rotation.eulerAngles;
            Initialize();
        }

        public void SetDecal(DecalEffect effect)
        {
            _textureAsset = ContentManager.Instance.GetAsset<ScenegraphTextureAsset>(new ResourceKey($"{effect.TextureName}_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR));
        }

        public void Initialize()
        {
            if (_material != null)
                _material.Free();
            var nhoodTerrain = NeighborhoodTerrain.Instance;
            if (nhoodTerrain == null)
                return;
            var terrainMeshFilter = nhoodTerrain.GetComponent<MeshFilter>();

            var meshFilter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();

            meshFilter.sharedMesh = terrainMeshFilter.sharedMesh;

            _material = new Material(Shader.Find("OpenTS2/NeighborhoodDecal"));
            _material.mainTexture = _textureAsset.GetSelectedImageAsUnityTexture();
            _material.SetVector(Location, _position);
            _material.SetVector(Rotation, _rotation);

            meshRenderer.sharedMaterial = _material;
            transform.SetParent(null);
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void OnDestroy()
        {
            if (_material != null)
                _material.Free();
        }
    }
}
