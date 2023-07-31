using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Rendering
{
    public static class MaterialUtils
    {
        private static int s_shadowMap = Shader.PropertyToID("_ShadowMap");
        private static int s_terrainWidth = Shader.PropertyToID("_TerrainWidth");
        private static int s_terrainHeight = Shader.PropertyToID("_TerrainHeight");
        private static int s_seaLevel = Shader.PropertyToID("_SeaLevel");

        /// <summary>
        /// Send neighborhood lightmap textures and information to a shader.
        /// </summary>
        public static void SetNeighborhoodParameters(Material material)
        {
            var ngbh = NeighborhoodManager.CurrentNeighborhood;
            if (ngbh != null)
            {
                material.SetTexture(s_shadowMap, LightmapManager.ShadowMap);
                material.SetFloat(s_terrainWidth, ngbh.Terrain.Width);
                material.SetFloat(s_terrainHeight, ngbh.Terrain.Height);
                material.SetFloat(s_seaLevel, ngbh.Terrain.SeaLevel);
            }
        }
    }
}
