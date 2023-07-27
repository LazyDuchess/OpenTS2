using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Rendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTS2.Components;

namespace OpenTS2.Scenes
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshRenderer))]
    public class NeighborhoodTerrain : AssetReferenceComponent
    {
        public static NeighborhoodTerrain Instance;
        public Transform Sun;
        private static ResourceKey s_matCapKey = new ResourceKey(0x0BE702EF, 0x8BA01057, TypeIDs.IMG);
        private static ResourceKey s_cliffKey = new ResourceKey(0xFFF56CAE, 0x6E80B6A1, 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        private static ResourceKey s_shoreKey = new ResourceKey("neighborhood-terrain-moisture-9_txtr", 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        //private static ResourceKey TemperateWetKey = new ResourceKey(0xFF354609, 0x1A9C59CC, 0x1C0532FA, TypeIDs.SCENEGRAPH_TXTR);
        private Material _terrainMaterial;
        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
            var contentProvider = ContentProvider.Get();
            var meshRenderer = GetComponent<MeshRenderer>();
            _terrainMaterial = meshRenderer.material;
            _terrainMaterial.SetVector("_LightVector", Sun.forward);

            var terrainType = NeighborhoodManager.CurrentNeighborhood.Terrain.TerrainType;

            var matCap = contentProvider.GetAsset<TextureAsset>(s_matCapKey);
            var smooth = contentProvider.GetAsset<ScenegraphTextureAsset>(terrainType.Texture);
            var variation1 = contentProvider.GetAsset<ScenegraphTextureAsset>(terrainType.Texture1);
            var variation2 = contentProvider.GetAsset<ScenegraphTextureAsset>(terrainType.Texture2);
            var cliff = contentProvider.GetAsset<ScenegraphTextureAsset>(s_cliffKey);
            var shore = contentProvider.GetAsset<ScenegraphTextureAsset>(s_shoreKey);
            AddReference(matCap, smooth, variation1, variation2, cliff, shore);
            if (matCap != null)
            {
                matCap.Texture.wrapMode = TextureWrapMode.Clamp;
                _terrainMaterial.SetTexture("_MatCap", matCap.Texture);
            }
            _terrainMaterial.mainTexture = smooth.GetSelectedImageAsUnityTexture(contentProvider);
            _terrainMaterial.SetTexture("_Variation1", variation1.GetSelectedImageAsUnityTexture(contentProvider));
            _terrainMaterial.SetTexture("_Variation2", variation2.GetSelectedImageAsUnityTexture(contentProvider));
            _terrainMaterial.SetTexture("_CliffTex", cliff.GetSelectedImageAsUnityTexture(contentProvider));
            _terrainMaterial.SetTexture("_Shore", shore.GetSelectedImageAsUnityTexture(contentProvider));
            _terrainMaterial.SetFloat("_SeaLevel", NeighborhoodManager.CurrentNeighborhood.Terrain.SeaLevel);
            SetTerrainMesh();
        }

        private void OnDestroy()
        {
            if (_terrainMaterial != null)
                _terrainMaterial.Free();
        }

        void SetTerrainMesh()
        {
            var terrainAsset = NeighborhoodManager.CurrentNeighborhood.Terrain;
            var terrainMesh = terrainAsset.MakeMesh();
            var meshFilter = GetComponent<MeshFilter>();
            var meshCollider = GetComponent<MeshCollider>();
            var meshRenderer = GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
            InitializeVertexColors(terrainMesh);
            var vars1 = GetVariationRectangles(terrainAsset.Width, terrainAsset.Height);
            var vars2 = GetVariationRectangles(terrainAsset.Width, terrainAsset.Height);
            
            MakeVertexColors(terrainMesh, vars1, vars2);
            LightmapManager.RenderShadowMap();
            meshRenderer.material.SetTexture("_ShadowMap", LightmapManager.ShadowMap);
            meshRenderer.material.SetTexture("_ShoreMask", LightmapManager.ShoreMap);
        }

        List<Rect> GetVariationRectangles(int width, int height)
        {
            var rectList = new List<Rect>();
            var amount = 8;
            for (var i = 0; i < amount; i++)
            {
                var w = Random.Range(20, 40);
                var h = Random.Range(20, 40);
                var xPos = Random.Range(0, width);
                var yPos = Random.Range(0, height);
                var rect = new Rect(new Vector2(xPos * 10, yPos * 10), new Vector2((xPos + w) * 10, (yPos + h) * 10));
                rectList.Add(rect);
            }
            return rectList;
        }

        void InitializeVertexColors(Mesh terrainMesh)
        {
            var verts = terrainMesh.vertices;
            var cols = new Color[verts.Length];
            for(var i=0;i<cols.Length;i++)
            {
                cols[i] = Color.black;
            }
            terrainMesh.colors = cols;
        }

        void MakeVertexColors(Mesh terrainMesh, List<Rect> variations1, List<Rect> variations2)
        {
            var seaLevel = NeighborhoodManager.CurrentNeighborhood.Terrain.SeaLevel;
            var vertices = terrainMesh.vertices;
            var colors = terrainMesh.colors;
            for(var i=0;i<vertices.Length;i++)
            {
                var vertex = vertices[i];
                var color = colors[i];
                foreach(var rect in variations1)
                {
                    if (vertex.x >= rect.x && vertex.z >= rect.y && vertex.x <= rect.width && vertex.z <= rect.height)
                    {
                        color.r = 1f;
                        break;
                    }
                }
                foreach (var rect in variations2)
                {
                    if (vertex.x >= rect.x && vertex.z >= rect.y && vertex.x <= rect.width && vertex.z <= rect.height)
                    {
                        color.g = 1f;
                        break;
                    }
                }
                colors[i] = color;
            }
            terrainMesh.colors = colors;
        }
    }
}
