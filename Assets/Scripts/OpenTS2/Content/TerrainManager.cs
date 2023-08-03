using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Content
{
    public static class TerrainManager
    {
        private static Dictionary<string, TerrainType> s_terrainTypes = new Dictionary<string, TerrainType>();
        public static void Initialize()
        {
            var temperate = new TerrainType
            {
                Name = "Temperate",
                Texture = new ResourceKey("nh-temperate-wet-00_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Texture1 = new ResourceKey("nh-temperate-wet-01_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Texture2 = new ResourceKey("nh-temperate-wet-01_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness = new ResourceKey("nh-temperate-drydry-00_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness1 = new ResourceKey("nh-temperate-drydry-01_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness2 = new ResourceKey("nh-temperate-drydry-02_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR)

            };
            var desert = new TerrainType
            {
                Name = "Desert",
                TerrainShader = Shader.Find("OpenTS2/DesertTerrain"),
                RoadDistanceForRoughness = 20f,
                RoughnessFalloff = 80f,
                MakeVariation = false,
                Texture = new ResourceKey("desert-smooth_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Texture1 = new ResourceKey("desert-medium_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Texture2 = new ResourceKey("desert-medium_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness = new ResourceKey("desert-rough_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness1 = new ResourceKey("desert-rough-red_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness2 = new ResourceKey("desert-rough-red_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                RoadTextureName = "desert_roads_{0}_txtr"
            };
            var concrete = new TerrainType
            {
                Name = "Concrete",
                MakeVariation = false,
                Texture = new ResourceKey("concrete-smooth_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Texture1 = new ResourceKey("concrete-smooth_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Texture2 = new ResourceKey("concrete-smooth_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness = new ResourceKey("dirt-grey_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness1 = new ResourceKey("dirt-grey_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness2 = new ResourceKey("dirt-grey_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                RoadTextureName = "concrete_roads_{0}_txtr"
            };
            var dirt = new TerrainType
            {
                Name = "Dirt",
                TerrainShader = Shader.Find("OpenTS2/DirtTerrain"),
                RoadDistanceForRoughness = 20f,
                RoughnessFalloff = 80f,
                MakeVariation = false,
                Texture = new ResourceKey("dirt-rough_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Texture1 = new ResourceKey("dirt-green_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Texture2 = new ResourceKey("dirt-green_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness = new ResourceKey("dirt-green-brown_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness1 = new ResourceKey("dirt-rough-light_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                Roughness2 = new ResourceKey("dirt-green-brown_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR),
                RoadTextureName = "dirt_roads_{0}_txtr"
            };
            RegisterTerrainType(temperate);
            RegisterTerrainType(desert);
            RegisterTerrainType(concrete);
            RegisterTerrainType(dirt);
        }

        public static void RegisterTerrainType(TerrainType type)
        {
            s_terrainTypes[type.Name] = type;
        }

        public static TerrainType GetTerrainType(string key)
        {
            if (!s_terrainTypes.TryGetValue(key, out TerrainType result))
                throw new KeyNotFoundException($"Can't find terrain type {key}");
            return result;
        }
    }
}
