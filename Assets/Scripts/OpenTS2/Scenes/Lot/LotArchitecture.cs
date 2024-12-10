using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.Lot.Roof;
using OpenTS2.Scenes.Lot.State;
using System;
using System.Collections.Generic;
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

        // Components
        private LotRoofComponent _roofComponent;
        private LotWallComponent _wallComponent;
        private LotFloorComponent _floorComponent;
        private LotTerrainComponent _terrainComponent;

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

            RoofAsset = lotPackage.GetAssetByTGI<RoofAsset>(new ResourceKey(RoofInstance, uint.MaxValue, TypeIDs.LOT_ROOF));
            Pool = lotPackage.GetAssetByTGI<PoolAsset>(new ResourceKey(PoolInstance, uint.MaxValue, TypeIDs.LOT_POOL));

            // Terrain

            BlendTextures = lotPackage.GetAssetByTGI<LotTexturesAsset>(new ResourceKey(BlendTexturesInstance, uint.MaxValue, TypeIDs.LOT_TEXTURES));
            WaterHeightmap = lotPackage.GetAssetByTGI<_2DArrayAsset>(new ResourceKey(WaterHeightmapInstance, uint.MaxValue, TypeIDs.LOT_TERRAIN)).GetView<float>(true);

            int textureCount = BlendTextures.BlendTextures.Length;
            BlendMasks = new _2DArrayView<byte>[textureCount];

            for (int i = 0; i < textureCount; i++)
            {
                BlendMasks[i] = lotPackage.GetAssetByTGI<_2DArrayAsset>(new ResourceKey((uint)(BlendMaskBaseInstance + i), uint.MaxValue, TypeIDs.LOT_TERRAIN)).GetView<byte>(true);
            }

            // Runtime structures

            BaseFloor = WallGraphAll.BaseFloor;
            Roof = new RoofCollection(RoofAsset.Entries, Elevation, BaseFloor);

            Validate();
        }

        private void Validate()
        {
            if (FloorPatterns.Width != Elevation.Width - 1 || FloorPatterns.Height != Elevation.Height - 1 || FloorPatterns.Depth != Elevation.Depth)
            {
                throw new InvalidOperationException("Size mismatch between heightmap and LTTX");
            }

            // Some terrain constraints...
            // Heightmap size must match size in textures, and all blend sizes must be 4x (-1) on both axis.
            // Blend textures must have the same count as the # of blend masks.

            if (BlendTextures.Width != Elevation.Width - 1 || BlendTextures.Height != Elevation.Height - 1)
            {
                throw new InvalidOperationException("Size mismatch between elevation and LTTX");
            }

            if (WaterHeightmap.Width != Elevation.Width || WaterHeightmap.Height != Elevation.Height)
            {
                throw new InvalidOperationException("Size mismatch between elevation and water heightmap");
            }

            if (BlendTextures.BlendTextures.Length != BlendMasks.Length)
            {
                throw new InvalidOperationException("Blend texture count mismatch between LOTG and LTTX");
            }

            foreach (var mask in BlendMasks)
            {
                if (mask.Width != Elevation.Width * 4 - 3 || mask.Height != Elevation.Height * 4 - 3)
                {
                    throw new InvalidOperationException("Size mismatch between mask and heightmap");
                }
            }
        }

        public T GetComponentByType<T>(ArchitectureGameObjectTypes Type) where T : AssetReferenceComponent => Type switch
        {
            ArchitectureGameObjectTypes.roof => _roofComponent as T,
            ArchitectureGameObjectTypes.wall => _wallComponent as T,
            ArchitectureGameObjectTypes.floor => _floorComponent as T,
            ArchitectureGameObjectTypes.terrain => _terrainComponent as T,
            _ => default
        };

        public void InvalidateComponent(ArchitectureGameObjectTypes Type) => GetComponentByType<AssetReferenceComponent>(Type)?.Invalidate();

        /// <summary>
        /// Creates game objects for the lot architecture, and returns them in the provided list.
        /// </summary>
        /// <param name="componentList">List of created game objects</param>
        public void CreateGameObjects(List<GameObject> componentList)
        {
            // make gameobjects for each type of object on the lot
            for(int type = 0; type < Enum.GetValues(typeof(ArchitectureGameObjectTypes)).Length; type++)
            {
                ArchitectureGameObjectTypes tType = (ArchitectureGameObjectTypes)type;
                if (!CreateGameObjectsOfType(componentList, tType))
                    ; // TODO: Handle if this fails 
            }
        }

        public enum ArchitectureGameObjectTypes
        {
            roof,
            wall, 
            floor, 
            terrain
        }

        /// <summary>
        /// Creates the correspondent GameObject for the type of geometry specified by <paramref name="Type"/>
        /// <para>Adds it to the given component list automatically</para>
        /// </summary>
        /// <param name="componentList"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public bool CreateGameObjectsOfType(List<GameObject> componentList, ArchitectureGameObjectTypes Type)
        {
            GameObject obj = default;
            switch (Type)
            {
                case ArchitectureGameObjectTypes.roof:
                    var roofObj = obj = new GameObject("roof", typeof(LotRoofComponent));
                    _roofComponent = roofObj.GetComponent<LotRoofComponent>().CreateFromLotArchitecture(this);
                    break;
                case ArchitectureGameObjectTypes.wall:
                    var wall = obj = new GameObject("wall", typeof(LotWallComponent));
                    _wallComponent = wall.GetComponent<LotWallComponent>().CreateFromLotArchitecture(this);
                    break;
                case ArchitectureGameObjectTypes.floor:
                    var floor = obj = new GameObject("floor", typeof(LotFloorComponent));
                    _floorComponent = floor.GetComponent<LotFloorComponent>().CreateFromLotArchitecture(this);
                    break;
                case ArchitectureGameObjectTypes.terrain:
                    var terrain = obj = new GameObject("terrain", typeof(LotTerrainComponent));
                    _terrainComponent = terrain.GetComponent<LotTerrainComponent>().CreateFromLotArchitecture(this);
                    break;
                default: return false;
            }
            componentList.Add(obj);
            return true;
        }

        private static int CalculateElevationIndex(int height, int x, int y)
        {
            return x * height + y;
        }

        private static float GetElevationInterp(float[] elevation, int width, int height, float x, float y)
        {
            int wm1 = width - 1;
            int hm1 = height - 1;

            x = Mathf.Clamp(x, 0, wm1);
            y = Mathf.Clamp(y, 0, hm1);

            float i0 = elevation[CalculateElevationIndex(height, (int)x, (int)y)];
            float i1 = elevation[CalculateElevationIndex(height, Math.Min(wm1, (int)x + 1), (int)y)];
            float j0 = elevation[CalculateElevationIndex(height, (int)x, Math.Min(hm1, (int)y + 1))];
            float j1 = elevation[CalculateElevationIndex(height, Math.Min(wm1, (int)x + 1), Math.Min(hm1, (int)y + 1))];

            float xi = x % 1;
            float yi = y % 1;

            return Mathf.Lerp(Mathf.Lerp(i0, i1, xi), Mathf.Lerp(j0, j1, xi), yi);
        }

        public int GetLevelAt(Vector3 position)
        {
            for (int i = 0; i < Elevation.Depth; i++)
            {
                float height = GetElevationInterp(Elevation.Data[i], Elevation.Width, Elevation.Height, position.x, position.y);

                if (position.z < height)
                {
                    return Math.Max(0, i - 1) + BaseFloor;
                }
            }

            return BaseFloor + Elevation.Depth - 1;
        }

        /// <summary>
        /// Update the world state, which determines how architecture is displayed.
        /// </summary>
        /// <param name="state">New world state</param>
        public void UpdateState(WorldState state)
        {
            _roofComponent.UpdateDisplay(state);
            _wallComponent.UpdateDisplay(state);
            _floorComponent.UpdateDisplay(state);
        }        
    }
}