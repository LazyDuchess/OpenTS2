using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTS2.Scenes;
using OpenTS2.Engine;
using OpenTS2.Content;

namespace OpenTS2.Rendering
{
    /// <summary>
    /// Generates Neighborhood shadows.
    /// </summary>
    public static class LightmapManager
    {
        public const float HeightDivision = 1000f;
        public static RenderTexture ShadowMap { get; private set; }
        public static RenderTexture ShoreMap { get; private set; }
        private static RenderTexture HeightMap;
        private static Shader HeightMapShader = Shader.Find("OpenTS2/TerrainHeightmap");
        private static Material HeightMapMaterial = new Material(HeightMapShader);
        private static Shader ShadowMapShader = Shader.Find("OpenTS2/HeightMapShadows");
        private static Material ShadowMapMaterial = new Material(ShadowMapShader);
        private static Shader ShoreMapShader = Shader.Find("OpenTS2/ShoreMask");
        private static Material ShoreMapMaterial = new Material(ShoreMapShader);
        private static int ShadowMapResolution = 256;
        private static int HeightMapResolution = 256;
        private static int ShoreResolution = 64;

        /// <summary>
        /// Renders lightmapping for the current neighborhood.
        /// </summary>
        public static void RenderShadowMap()
        {
            var terrain = NeighborhoodTerrain.Instance;
            var meshFilter = terrain.GetComponent<MeshFilter>();
            var sun = terrain.Sun;
            var mesh = meshFilter.sharedMesh;
            var neighborhood = NeighborhoodManager.Instance.CurrentNeighborhood;

            if (HeightMap == null)
                HeightMap = new RenderTexture(HeightMapResolution, HeightMapResolution, 16, RenderTextureFormat.R16);
            RenderTexture.active = HeightMap;
            HeightMapMaterial.SetPass(0);
            HeightMapMaterial.SetFloat("_HeightDivision", HeightDivision);
            HeightMapMaterial.SetFloat("_Width", neighborhood.Terrain.Width);
            HeightMapMaterial.SetFloat("_Height", neighborhood.Terrain.Height);
            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
            RenderTexture.active = null;

            if (ShadowMap == null)
                ShadowMap = new RenderTexture(ShadowMapResolution, ShadowMapResolution, 16, RenderTextureFormat.R16);
            RenderTexture.active = ShadowMap;
            ShadowMapMaterial.mainTexture = HeightMap;
            ShadowMapMaterial.SetVector("_LightVector", sun.forward);
            Graphics.Blit(HeightMap, ShadowMapMaterial);
            RenderTexture.active = null;

            if (ShoreMap == null)
                ShoreMap = new RenderTexture(ShoreResolution, ShoreResolution, 16, RenderTextureFormat.R16);
            RenderTexture.active = ShoreMap;
            ShoreMapMaterial.mainTexture = HeightMap;
            ShoreMapMaterial.SetFloat("_SeaLevel", neighborhood.Terrain.SeaLevel / HeightDivision);
            Graphics.Blit(HeightMap, ShoreMapMaterial);
            RenderTexture.active = null;
        }
    }
}