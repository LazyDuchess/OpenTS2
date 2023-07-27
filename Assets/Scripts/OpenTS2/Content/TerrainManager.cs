using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public static class TerrainManager
    {
        private static Dictionary<string, TerrainType> s_terrainTypes = new Dictionary<string, TerrainType>();
        public static void Initialize()
        {
            RegisterTerrainType<TemperateTerrain>();
            RegisterTerrainType<DesertTerrain>();
            RegisterTerrainType<ConcreteTerrain>();
            RegisterTerrainType<DirtTerrain>();
        }

        public static void RegisterTerrainType<T>() where T : TerrainType
        {
            var instance = Activator.CreateInstance(typeof(T)) as TerrainType;
            s_terrainTypes[instance.Name] = instance;
        }

        public static TerrainType GetTerrainType(string key)
        {
            if (!s_terrainTypes.TryGetValue(key, out TerrainType result))
                return s_terrainTypes["Temperate"];
            return result;
        }
    }
}
