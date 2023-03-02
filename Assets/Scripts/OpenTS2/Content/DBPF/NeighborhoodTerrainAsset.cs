using System;
using OpenTS2.Files.Formats.DBPF.Types;
using UnityEngine;

namespace OpenTS2.Content.DBPF
{
    public class NeighborhoodTerrainAsset : AbstractAsset
    {
        public int Width { get; }
        public int Height { get; }
        public float SeaLevel { get; }
        public string TerrainType { get; }
        public FloatArray2D VertexHeights { get; }

        public NeighborhoodTerrainAsset(int width, int height, float seaLevel, string terrainType,
            FloatArray2D vertexHeights) =>
            (Width, Height, SeaLevel, TerrainType, VertexHeights) =
            (width, height, seaLevel, terrainType, vertexHeights);

        public void ApplyToTerrain(Terrain terrain)
        {
            var heightMap = new float[VertexHeights.values.GetLength(1), VertexHeights.values.GetLength(0)];
            for (var i = 0; i < VertexHeights.values.GetLength(0); i++)
            {
                for (var j = 0; j < VertexHeights.values.GetLength(1); j++)
                {
                    // TODO: divide by the max-height here to get a proper 0 to 1 float. Picked 600 arbitrarily for now.
                    heightMap[j, i] = VertexHeights.values[i, j] / 600.0f;
                }
            }

            terrain.terrainData = new TerrainData
            {
                size = new Vector3(4096, 4096, 4096),
                heightmapResolution = Math.Max(VertexHeights.values.GetLength(0), VertexHeights.values.GetLength(1))
            };
            terrain.terrainData.SetHeights(0, 0, heightMap);
            terrain.terrainData.SyncHeightmap();
        }
    }
}