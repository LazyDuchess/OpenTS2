using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshRenderer))]
    public class NeighborhoodTerrain : MonoBehaviour
    {
        public float RayDistance = 2000f;
        public float RayBias = 1f;
        public float BlurOffset = 10f;
        public Transform Sun;
        public bool Reload = false;
        public bool Realtime = false;
        private static ResourceKey DayTimeMatCapKey = new ResourceKey(0x0BE702EF, 0x8BA01057, TypeIDs.IMG);
        private static ResourceKey TemperateWetKey = new ResourceKey(0xFFFB3AC6, 0x4B3FEBD4, 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        private static ResourceKey CliffKey = new ResourceKey(0xFFF56CAE, 0x6E80B6A1, 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        //private static ResourceKey TemperateWetKey = new ResourceKey(0xFF354609, 0x1A9C59CC, 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        private Material _terrainMaterial;
        private TextureAsset _matcapAsset;
        private ScenegraphTextureAsset _temperateWetTextureAsset;
        private ScenegraphTextureAsset _cliffTextureAsset;
        // Start is called before the first frame update
        void Start()
        {
            var contentProvider = ContentProvider.Get();
            var meshRenderer = GetComponent<MeshRenderer>();
            _terrainMaterial = meshRenderer.material;
            _terrainMaterial.SetVector("_LightVector", Sun.forward);
            _matcapAsset = contentProvider.GetAsset<TextureAsset>(DayTimeMatCapKey);
            _temperateWetTextureAsset = contentProvider.GetAsset<ScenegraphTextureAsset>(TemperateWetKey);
            _cliffTextureAsset = contentProvider.GetAsset<ScenegraphTextureAsset>(CliffKey);
            if (_matcapAsset != null)
            {
                _matcapAsset.Texture.wrapMode = TextureWrapMode.Clamp;
                _terrainMaterial.SetTexture("_MatCap", _matcapAsset.Texture);
                _terrainMaterial.SetTexture("_CliffTex", _cliffTextureAsset.GetSelectedImageAsUnityTexture(contentProvider));
                _terrainMaterial.mainTexture = _temperateWetTextureAsset.GetSelectedImageAsUnityTexture(contentProvider);
            }
            SetTerrainMesh();
        }

        private void OnDestroy()
        {
            if (_terrainMaterial != null)
                _terrainMaterial.Free();
        }

        private void Update()
        {
            if (Reload || Realtime)
            {
                _terrainMaterial.SetVector("_LightVector", Sun.forward);
                var meshFilter = GetComponent<MeshFilter>();
                MakeVertexColors(meshFilter.sharedMesh);
                Reload = false;
            }
        }

        void SetTerrainMesh()
        {
            var terrainAsset = NeighborhoodManager.CurrentNeighborhood.Terrain;
            var terrainMesh = terrainAsset.MakeMesh();
            var meshFilter = GetComponent<MeshFilter>();
            var meshCollider = GetComponent<MeshCollider>();
            meshFilter.sharedMesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
            MakeVertexColors(terrainMesh);
        }

        void MakeVertexColors(Mesh terrainMesh)
        {
            var vertices = terrainMesh.vertices;
            var normals = terrainMesh.normals;
            var colors = new Color[vertices.Length];
            for(var i=0;i<vertices.Length;i++)
            {
                var vertex = vertices[i];
                var normal = normals[i];
                var color = Color.black;
                var shadowAccumulator = 0f;
                for (var j=-1;j<2;j++)
                {
                    for(var n=-1;n<2;n++)
                    {
                        var offX = BlurOffset * j;
                        var offZ = BlurOffset * n;
                        var rayTarget = vertex + (normal * RayBias) + offX * Vector3.right + offZ * Vector3.forward;
                        var ray = new Ray(rayTarget, -Sun.forward);
                        if (Physics.Raycast(ray, RayDistance))
                            shadowAccumulator += 1f;
                    }
                }
                shadowAccumulator /= 9f;
                color.r = shadowAccumulator;
                colors[i] = color;
            }
            terrainMesh.colors = colors;
        }
    }
}
