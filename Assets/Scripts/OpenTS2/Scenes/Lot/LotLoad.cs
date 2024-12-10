// JDrocks450 11-22-2023 on GitHub

#define NEW_INVALIDATE
#undef NEW_INVALIDATE

using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.Lot.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    /// <summary>
    /// Standard behavior for loading into the lot view.
    /// <para>Provides standard functionality for placing terrain, walls, floors, objects, etc. into the Scene</para>
    /// </summary>
    public class LotLoad : MonoBehaviour
    {
        #region private
        private WorldState _state = new WorldState(1, WallsMode.Roof);

        private string _nhood;
        private int _lotId;

        private DBPFFile _lotPackage;

        private List<GameObject> _lotObject = new List<GameObject>();
        private List<GameObject> _testObjects = new List<GameObject>();

        private RuntimeWallController _wallController = new RuntimeWallController();
        #endregion

        #region public
        /// <summary>
        /// Settings to dictate how the lot should be loaded
        /// </summary>
        public struct LotLoadSettings
        {
            [Flags]
            public enum LotViewLayers : int
            {
                /// <summary>
                /// Default - Do not display any architecture or objects
                /// </summary>
                None = 0,
                /// <summary>
                /// Flag - Loads terrain when set
                /// </summary>
                Terrain = 1,
                /// <summary>
                /// Flag - Loads walls when set
                /// </summary>
                Walls = 2,
                /// <summary>
                /// Flag - Loads floors when set
                /// </summary>
                Floors = 4,
                /// <summary>
                /// Flag - Loads objects when set
                /// </summary>
                Objects = 8
            }
            /// <summary>
            /// Generates the flags necessary to load all layers
            /// </summary>
            public const LotViewLayers AllLayers = 
                LotViewLayers.None | LotViewLayers.Terrain | LotViewLayers.Walls | LotViewLayers.Floors | LotViewLayers.Objects;
            /// <summary>
            /// Specifies which layers of the lot geometry should be loaded
            /// </summary>
            public LotViewLayers LoadLayers;
            /// <summary>
            /// Makes a new <see cref="LotLoadSettings"/> using the specified layers.
            /// <para/> Default is <see cref="AllLayers"/> value -- to load all layers on the lot
            /// </summary>
            /// <param name="loadLayers"></param>
            public LotLoadSettings(LotViewLayers loadLayers = AllLayers)
            {
                LoadLayers = loadLayers;
            }
        }
        /// <summary>
        /// (Debug) <c>N001</c>
        /// </summary>
        public const string Default_NeighborhoodPrefix = "N001";
        /// <summary>
        /// (Debug) <c>82</c>
        /// </summary>
        public const int Default_LotID = 82;

        /// <summary>
        /// The current floor selected in the <see cref="WorldState"/>
        /// </summary>
        public int Floor => _state.Level;
        /// <summary>
        /// The current ViewMode in the <see cref="WorldState"/>
        /// <para/><see cref="WorldState.Walls"/>
        /// </summary>
        public WallsMode ViewMode = WallsMode.Roof;

        public int BaseFloor => Architecture?.BaseFloor ?? 0;
        public int MaxFloor => Architecture?.FloorPatterns.Depth ?? 1;
        /// <summary>
        /// The current <see cref="WorldState"/> which indicates how the level should appear
        /// <para/>Changing this directly will log in the console, but should be followed up with <see cref="InvalidateState"/> to ensure 
        /// changes can be perceived in the world.
        /// </summary>
        public WorldState WorldState
        {
            get => _state;
            set
            {
                _state = value;
                Debug.Log($"World state updated: {value}");
            }
        }        

        /// <summary>
        /// The currently selected Neighborhood Prefix (N001 by default)
        /// </summary>
        public string NeighborhoodPrefix => _nhood;
        /// <summary>
        /// The currently selected Lot ID in the <see cref="NeighborhoodPrefix"/> selected (82 by default)
        /// </summary>
        public int LotID => _lotId;
        /// <summary>
        /// The <see cref="LotArchitecture"/> base that this <see cref="LotLoad"/> instance used to load the scene
        /// </summary>
        public LotArchitecture Architecture { get; private set; }
        #endregion

        /// <summary>
        /// Loads the default lot <para/>
        /// See: <see cref="Default_NeighborhoodPrefix"/> and <see cref="Default_LotID"/>
        /// </summary>
        public LotLoad() : this(Default_NeighborhoodPrefix, Default_LotID) { }
        /// <summary>
        /// Creates a new <see cref="LotLoad"/> instance (that can be reused) with the specified Neighborhood and LotID to load
        /// </summary>
        /// <param name="Nhood"></param>
        /// <param name="LotID"></param>
        public LotLoad(string Nhood, int LotID)
        {            
            LoadLot(Nhood, LotID);
        }
        /// <summary>
        /// Unloads the current lot and destroys each object instance in memory.
        /// </summary>
        public void UnloadLot()
        {
            foreach (var obj in _lotObject)
            {
                Destroy(obj);
            }

            _lotObject.Clear();
            _testObjects.Clear();
        }

        /// <summary>
        /// Updates <see cref="NeighborhoodPrefix"/> and <see cref="LotID"/> and loads the lot.
        /// See: <see cref="LoadLot(LotLoadSettings)"/>
        /// </summary>
        /// <param name="neighborhoodPrefix"></param>
        /// <param name="id"></param>
        public void LoadLot(string neighborhoodPrefix, int id, LotLoadSettings Settings = default)
        {
            _nhood = neighborhoodPrefix;
            _lotId = id;

            LoadLot(Settings);
        }
        /// <summary>
        /// Unloads the current lot (if applicable) and loads the lot provided by <paramref name="Settings"/>
        /// </summary>
        /// <param name="Settings">The settings to use to load the lot</param>
        public void LoadLot(LotLoadSettings Settings = default)
        {
            //Unload previous session
            UnloadLot();

            var contentProvider = ContentProvider.Get();
            //Find package file for the lot given
            var lotsFolderPath = Path.Combine(Filesystem.GetUserPath(), $"Neighborhoods/{NeighborhoodPrefix}/Lots");
            var lotFilename = $"{NeighborhoodPrefix}_Lot{LotID}.package";
            var lotFullPath = Path.Combine(lotsFolderPath, lotFilename);

            if (!File.Exists(lotFullPath))
            {
                throw new FileNotFoundException($"LoadLot(): File not found. {lotFullPath}");
            }

            _lotPackage = contentProvider.AddPackage(lotFullPath);

            //add objects from scenegraph (primitive need to change to more robust solution later)
            InvalidateObjects();

            Architecture = new LotArchitecture();

            Architecture.LoadFromPackage(_lotPackage);
            Architecture.CreateGameObjects(_lotObject);

            ViewMode = WallsMode.Roof; // Default value is Roof
            _state = new WorldState(Floor, ViewMode);
            InvalidateState();
        }
        /// <summary>
        /// Flushes all currently loaded objects and reloads them from package
        /// </summary>
        public void InvalidateObjects()
        {
            //Clear any objects on the scene
            UnloadLot();

            var contentProvider = ContentProvider.Get();
            // Go through each lot object.
            foreach (var entry in _lotPackage.Entries)
            {
                if (entry.GlobalTGI.TypeID != TypeIDs.LOT_OBJECT)
                {
                    continue;
                }

                try
                {
                    var lotObject = entry.GetAsset<LotObjectAsset>();
                    var resource = contentProvider.GetAsset<ScenegraphResourceAsset>(
                        new ResourceKey(lotObject.Object.ResourceName + "_cres", GroupIDs.Scenegraph,
                            TypeIDs.SCENEGRAPH_CRES));
                    if (resource == null)
                    {
                        Debug.Log($"Could not find lot object: {lotObject.Object.ResourceName}");
                        continue;
                    }

                    var model = resource.CreateRootGameObject();
                    model.transform.GetChild(0).localPosition = lotObject.Object.Position;
                    model.transform.GetChild(0).localRotation = lotObject.Object.Rotation;

                    _lotObject.Add(model);
                    _testObjects.Add(model);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        /// <summary>
        /// Invalidates the specified group supplied by <paramref name="group"/>
        /// </summary>
        /// <param name="group"></param>
        void BaseInvalidateGroup(LotArchitecture.ArchitectureGameObjectTypes group)
        {
#if NEW_INVALIDATE
            architecture.InvalidateComponent(group);
            return;
#endif
            if (group == LotArchitecture.ArchitectureGameObjectTypes.roof)
            {
                Architecture.InvalidateComponent(group);
                return;
            }
            //old
            string objectName = group.ToString();
            //destroy existing object
            var groupParent = _lotObject.Where(x => x.name == objectName).FirstOrDefault();
            if (groupParent != null)
            { // clean walls
                Destroy(groupParent);
                _lotObject.Remove(groupParent);
            }
            //rebuild new mesh from memory
            Architecture.CreateGameObjectsOfType(_lotObject, group);
        }

        public void InvalidateFloors()
        {
            BaseInvalidateGroup(LotArchitecture.ArchitectureGameObjectTypes.floor);
            InvalidateTerrain();
        }
        public void InvalidateWalls() => BaseInvalidateGroup(LotArchitecture.ArchitectureGameObjectTypes.wall);
        public void InvalidateTerrain() => BaseInvalidateGroup(LotArchitecture.ArchitectureGameObjectTypes.terrain);

        /// <summary>
        /// After changing the <see cref="WorldState"/>, use this method to update the visual
        /// </summary>
        public void InvalidateState()
        {
            Architecture.UpdateState(_state);
            UpdateObjectVisibility(_state.Level);
        }
        /// <summary>
        /// Sets the given object's visibility to be Visible/Invisible
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="visible"></param>
        private void SetObjectVisiblity(GameObject obj, bool visible)
        {
            foreach (MeshRenderer renderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.shadowCastingMode = visible ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            foreach (SkinnedMeshRenderer renderer in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.shadowCastingMode = visible ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
        /// <summary>
        /// Sets objects to be visible based on if they should be seen by the player or not.
        /// <para/>This uses the <paramref name="viewLevel"/> to determine up to which floor an object should be viewable.
        /// </summary>
        /// <param name="viewLevel"></param>
        private void UpdateObjectVisibility(int viewLevel)
        {
            foreach (GameObject obj in _testObjects)
            {
                int level = Architecture.GetLevelAt(obj.transform.GetChild(0).localPosition);

                SetObjectVisiblity(obj, level < viewLevel);
            }
        }        

        /// <summary>
        /// Regenerates the RoomMap from scratch. [BETA]
        /// </summary>
        internal void EvaluateRoomMap()
        {
            _wallController.GenerateAll(Architecture);
        }
    }
}
