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
        private static int ShadowMap = Shader.PropertyToID("_ShadowMap");
        private static int TerrainWidth = Shader.PropertyToID("_TerrainWidth");
        private static int TerrainHeight = Shader.PropertyToID("_TerrainHeight");
        private static int SeaLevel = Shader.PropertyToID("_SeaLevel");

        /// <summary>
        /// Send neighborhood lightmap textures and information to a shader.
        /// </summary>
        public static void SetNeighborhoodParameters(Material material)
        {
            var ngbh = NeighborhoodManager.CurrentNeighborhood;
            if (ngbh != null)
            {
                material.SetTexture(ShadowMap, LightmapManager.ShadowMap);
                material.SetFloat(TerrainWidth, ngbh.Terrain.Width);
                material.SetFloat(TerrainHeight, ngbh.Terrain.Height);
                material.SetFloat(SeaLevel, ngbh.Terrain.SeaLevel);
            }
        }
    }
}
