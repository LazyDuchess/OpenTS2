using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTS2.Scenes;

namespace OpenTS2.Rendering
{
    public static class LightmapManager
    {
        public static RenderTexture ShadowMap => s_shadowMap;
        private static RenderTexture s_heightMap;
        private static Shader s_heightMapShader = Shader.Find("OpenTS2/TerrainHeightmap");
        private static Material s_heightMapMaterial = new Material(s_heightMapShader);
        private static RenderTexture s_shadowMap;
        private static Shader s_shadowMapShader = Shader.Find("OpenTS2/HeightMapShadows");
        private static Material s_shadowMapMaterial = new Material(s_shadowMapShader);
        private static int s_resolution = 128;
        public static void RenderShadowMap()
        {
            var terrain = GameObject.FindObjectOfType<NeighborhoodTerrain>();
            var meshFilter = terrain.GetComponent<MeshFilter>();
            var sun = terrain.Sun;
            var mesh = meshFilter.sharedMesh;

            s_heightMap = new RenderTexture(s_resolution, s_resolution, 16, RenderTextureFormat.R8);
            RenderTexture.active = s_heightMap;
            s_heightMapMaterial.SetPass(0);
            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
            RenderTexture.active = null;

            s_shadowMap = new RenderTexture(s_resolution, s_resolution, 16, RenderTextureFormat.R8);
            RenderTexture.active = s_shadowMap;
            s_shadowMapMaterial.mainTexture = s_heightMap;
            s_shadowMapMaterial.SetVector("_LightVector", sun.forward);
            Graphics.Blit(s_heightMap, s_shadowMapMaterial);
            RenderTexture.active = null;
        }
    }
}