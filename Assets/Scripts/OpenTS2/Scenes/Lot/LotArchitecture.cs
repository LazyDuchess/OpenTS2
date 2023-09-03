using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.Lot.Roof;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    public class LotArchitecture
    {
        public const int WallMapInstance = 13;
        public const int FloorMapInstance = 14;

        public const int WallLayerInstance = 4;
        public const int WallGraphAllInstance = 0x18;
        public const int FencePostLayerInstance = 0x11;

        public const int PoolInstance = 0;
        public const int RoofInstance = 0;

        public const int ArrayFloorPatternsInstance = 0;
        public const int ArrayElevationInstance = 1;
        public const int ArrayReservedTilesInstance = 3;
        public const int ArrayUnknown1Instance = 9;
        public const int ArrayUnknownRoomMap1Instance = 10;
        public const int ArrayUnknownRoomMap2Instance = 11;
        public const int ArrayUnknownIntInstance = 12;
        public const int ArrayUnknownInt2Instance = 20;
        public const int ArrayUnknownVector4ByteInstance = 21;
        public const int ArrayUnknownExpansionMaskInstance = 27; // All 0xFF by default
        public const int ArrayUnknownExpansionMask2Instance = 32; // All 0x11 by default
        public const int ArrayUnknownExpansionMask3Instance = 0x5D00; // Empty or missing by default
        public const int ArrayUnknownExpansionMask4Instance = 0x5D01; // All 0 by default, sometimes 0x0a
        public const int ArrayUnknownExpansionMask5Instance = 0x5D02; // All 0x11 by default

        public const int BlendTexturesInstance = 0;
        public const int WaterHeightmapInstance = 15222;
        public const int BlendMaskBaseInstance = 0x5CBC;
        public const int TerrainUnknownInstance = 0x5CEE;

        // Metadata
        public int BaseFloor { get; private set; }

        // Resource Map
        public StringMapAsset WallMap { get; private set; }
        public StringMapAsset FloorMap { get; private set; }

        // Walls
        public WallGraphAsset WallGraphAll { get; private set; }
        public WallLayerAsset WallLayer { get; private set; }
        public FencePostLayerAsset FencePostLayer { get; private set; }

        // 3D Arrays
        public _3DArrayView<Vector4<ushort>> FloorPatterns { get; private set; }
        public _3DArrayView<float> Elevation { get; private set; }

        public RoofAsset RoofAsset { get; private set; }
        public RoofCollection Roof { get; private set; }
        public PoolAsset Pool { get; private set; }

        // Terrain
        public LotTexturesAsset BlendTextures { get; private set; }
        public _2DArrayView<float> WaterHeightmap { get; private set; }
        public _2DArrayView<byte>[] BlendMasks { get; private set; }


        public void LoadFromPackage(DBPFFile lotPackage)
        {
            // TODO: Failsafes when assets do not exist.

            WallMap = lotPackage.GetAssetByTGI<StringMapAsset>(new ResourceKey(WallMapInstance, uint.MaxValue, TypeIDs.LOT_STRINGMAP));
            FloorMap = lotPackage.GetAssetByTGI<StringMapAsset>(new ResourceKey(FloorMapInstance, uint.MaxValue, TypeIDs.LOT_STRINGMAP));

            /*
             * There are a bunch of these that aren't used yet.
            var wallGraphs = lotPackage.Entries.Where(x => x.GlobalTGI.TypeID == TypeIDs.LOT_WALLGRAPH).Select(x => x.GetAsset<WallGraphAsset>()).ToList();
            var arry3 = lotPackage.Entries.Where(x => x.GlobalTGI.TypeID == TypeIDs.LOT_3ARY).Select(x => x.GetAsset()).ToList();
            */

            // Shared

            WallGraphAll = lotPackage.GetAssetByTGI<WallGraphAsset>(new ResourceKey(WallGraphAllInstance, uint.MaxValue, TypeIDs.LOT_WALLGRAPH));
            WallLayer = lotPackage.GetAssetByTGI<WallLayerAsset>(new ResourceKey(WallLayerInstance, uint.MaxValue, TypeIDs.LOT_WALLLAYER));
            FencePostLayer = lotPackage.GetAssetByTGI<FencePostLayerAsset>(new ResourceKey(FencePostLayerInstance, uint.MaxValue, TypeIDs.LOT_FENCEPOST));

            FloorPatterns = lotPackage.GetAssetByTGI<_3DArrayAsset>(new ResourceKey(ArrayFloorPatternsInstance, uint.MaxValue, TypeIDs.LOT_3ARY)).GetView<Vector4<ushort>>(true);
            Elevation = lotPackage.GetAssetByTGI<_3DArrayAsset>(new ResourceKey(ArrayElevationInstance, uint.MaxValue, TypeIDs.LOT_3ARY)).GetView<float>(true);

            Pool = lotPackage.GetAssetByTGI<PoolAsset>(new ResourceKey(PoolInstance, uint.MaxValue, TypeIDs.LOT_POOL));
            RoofAsset = lotPackage.GetAssetByTGI<RoofAsset>(new ResourceKey(RoofInstance, uint.MaxValue, TypeIDs.LOT_ROOF));

            BaseFloor = WallGraphAll.BaseFloor;

            Roof = new RoofCollection(RoofAsset.Entries, Elevation, BaseFloor);

            // Terrain

            BlendTextures = lotPackage.GetAssetByTGI<LotTexturesAsset>(new ResourceKey(BlendTexturesInstance, uint.MaxValue, TypeIDs.LOT_TEXTURES));
            WaterHeightmap = lotPackage.GetAssetByTGI<_2DArrayAsset>(new ResourceKey(WaterHeightmapInstance, uint.MaxValue, TypeIDs.LOT_TERRAIN)).GetView<float>(true);

            int textureCount = BlendTextures.BlendTextures.Length;
            BlendMasks = new _2DArrayView<byte>[textureCount];

            for (int i = 0; i < textureCount; i++)
            {
                BlendMasks[i] = lotPackage.GetAssetByTGI<_2DArrayAsset>(new ResourceKey((uint)(BlendMaskBaseInstance + i), uint.MaxValue, TypeIDs.LOT_TERRAIN)).GetView<byte>(true);
            }
        }

        /// <summary>
        /// Creates game objects for the lot architecture, and returns them in the provided list.
        /// </summary>
        /// <param name="componentList">List of created game objects</param>
        public void CreateGameObjects(List<GameObject> componentList)
        {
            var roofObj = new GameObject("roof", typeof(LotRoofComponent));
            roofObj.GetComponent<LotRoofComponent>().CreateFromLotAssets(RoofAsset.Entries.FirstOrDefault().Pattern, Roof);
            componentList.Add(roofObj);

            var wall = new GameObject("wall", typeof(LotWallComponent));
            wall.GetComponent<LotWallComponent>().CreateFromLotAssets(WallMap, WallLayer, WallGraphAll, FencePostLayer, Elevation, Roof);
            componentList.Add(wall);

            var floor = new GameObject("floor", typeof(LotFloorComponent));
            floor.GetComponent<LotFloorComponent>().CreateFromLotAssets(FloorMap, FloorPatterns, Elevation, BaseFloor);
            componentList.Add(floor);

            var terrain = new GameObject("terrain", typeof(LotTerrainComponent));
            terrain.GetComponent<LotTerrainComponent>().CreateFromTerrainAssets(BlendTextures, Elevation, BlendMasks, WaterHeightmap, FloorPatterns, BaseFloor);
            componentList.Add(terrain);
        }
    }
}