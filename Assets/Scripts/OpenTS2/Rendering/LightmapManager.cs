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
        public static RenderTexture ShadowMap => s_shadowMap;
        public static RenderTexture ShoreMap => s_shoreMap;
        private static RenderTexture s_heightMap;
        private static Shader s_heightMapShader = Shader.Find("OpenTS2/TerrainHeightmap");
        private static Material s_heightMapMaterial = new Material(s_heightMapShader);
        private static RenderTexture s_shadowMap;
        private static Shader s_shadowMapShader = Shader.Find("OpenTS2/HeightMapShadows");
        private static Material s_shadowMapMaterial = new Material(s_shadowMapShader);
        private static RenderTexture s_shoreMap;
        private static Shader s_shoreMapShader = Shader.Find("OpenTS2/ShoreMask");
        private static Material s_shoreMapMaterial = new Material(s_shoreMapShader);
        private static int s_shadowMapResolution = 128;
        private static int s_heightMapResolution = 128;
        private static int s_shoreResolution = 128;
        /// <summary>
        /// Renders lightmapping for the current neighborhood.
        /// </summary>
        public static void RenderShadowMap()
        {
            if (s_heightMap != null)
            {
                s_heightMap.Free();
                s_heightMap = null;
            }
            if (s_shadowMap != null)
            {
                s_shadowMap.Free();
                s_shadowMap = null;
            }
            if (s_shoreMap != null)
            {
                s_shoreMap.Free();
                s_shoreMap = null;
            }

            var terrain = GameObject.FindObjectOfType<NeighborhoodTerrain>();
            var meshFilter = terrain.GetComponent<MeshFilter>();
            var sun = terrain.Sun;
            var mesh = meshFilter.sharedMesh;
            var neighborhood = NeighborhoodManager.CurrentNeighborhood;

            s_heightMap = new RenderTexture(s_heightMapResolution, s_heightMapResolution, 16, RenderTextureFormat.R8);
            RenderTexture.active = s_heightMap;
            s_heightMapMaterial.SetPass(0);
            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
            RenderTexture.active = null;

            s_shadowMap = new RenderTexture(s_shadowMapResolution, s_shadowMapResolution, 16, RenderTextureFormat.R8);
            RenderTexture.active = s_shadowMap;
            s_shadowMapMaterial.mainTexture = s_heightMap;
            s_shadowMapMaterial.SetVector("_LightVector", sun.forward);
            Graphics.Blit(s_heightMap, s_shadowMapMaterial);
            RenderTexture.active = null;

            s_shoreMap = new RenderTexture(s_shoreResolution, s_shoreResolution, 16, RenderTextureFormat.R8);
            RenderTexture.active = s_shoreMap;
            s_shoreMapMaterial.mainTexture = s_heightMap;
            s_shoreMapMaterial.SetFloat("_SeaLevel", neighborhood.Terrain.SeaLevel / 1000);
            Graphics.Blit(s_heightMap, s_shoreMapMaterial);
            RenderTexture.active = null;
        }
    }
}