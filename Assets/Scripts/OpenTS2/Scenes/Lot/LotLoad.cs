// JDrocks450 11-22-2023 on GitHub

#define NEW_INVALIDATE
#undef NEW_INVALIDATE

using OpenTS2.Common;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Content.DBPF;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files;
using OpenTS2.Scenes.Lot.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    /// <summary>
    /// Standard behavior for loading into the lot view.
    /// <para>Provides standard functionality for placing terrain, walls, floors, objects, etc. into the Scene</para>
    /// </summary>
    public class LotLoad : MonoBehaviour
    {
        /// <summary>
        /// Settings for how the lot should be loaded
        /// </summary>
        public struct LotLoadSettings
        {
            [Flags]
            public enum LotViewLayers : int
            {
                None = 0,
                Terrain = 1,
                Walls = 2,
                Floors = 4,
                Objects = 8
            }
            public const LotViewLayers AllLayers = 
                LotViewLayers.None | LotViewLayers.Terrain | LotViewLayers.Walls | LotViewLayers.Floors | LotViewLayers.Objects;
            /// <summary>
            /// Specifies which layers of the lot geometry should be loaded
            /// </summary>
            public LotViewLayers LoadLayers;

            public LotLoadSettings(LotViewLayers loadLayers = AllLayers)
            {
                LoadLayers = loadLayers;
            }
        }

        public const string Default_NeighborhoodPrefix = "N001";
        public const int Default_LotID = 82;

        public int Floor => _state.Level;
        public WallsMode Mode = WallsMode.Roof;

        public int BaseFloor => architecture?.BaseFloor ?? 0;
        public int MaxFloor => architecture?.FloorPatterns.Depth ?? 1;

        public WorldState WorldState
        {
            get => _state;
            set
            {
                _state = value;
                Debug.Log($"World state updated: {value}");
            }
        }

        private WorldState _state = new WorldState(1, WallsMode.Roof);

        private string _nhood;
        private int _lotId;

        private DBPFFile lotPackage;

        /// <summary>
        /// The currently selected Neighborhood Prefix (N001 by default)
        /// </summary>
        public string NeighborhoodPrefix => _nhood;
        /// <summary>
        /// The currently selected Lot ID in the <see cref="NeighborhoodPrefix"/> selected (82 by default)
        /// </summary>
        public int LotID => _lotId;

        private List<GameObject> _lotObject = new List<GameObject>();
        private List<GameObject> _testObjects = new List<GameObject>();
        public LotArchitecture architecture;

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
        /// <see cref="LoadLot()"/>
        /// </summary>
        /// <param name="neighborhoodPrefix"></param>
        /// <param name="id"></param>
        public void LoadLot(string neighborhoodPrefix, int id, LotLoadSettings Settings = default)
        {
            _nhood = neighborhoodPrefix;
            _lotId = id;

            LoadLot(Settings);
        }

        public void LoadLot(LotLoadSettings Settings = default)
        {
            //Unload previous session
            UnloadLot();

            var contentProvider = ContentProvider.Get();

            var lotsFolderPath = Path.Combine(Filesystem.GetUserPath(), $"Neighborhoods/{NeighborhoodPrefix}/Lots");
            var lotFilename = $"{NeighborhoodPrefix}_Lot{LotID}.package";
            var lotFullPath = Path.Combine(lotsFolderPath, lotFilename);

            if (!File.Exists(lotFullPath))
            {
                return;
            }

            lotPackage = contentProvider.AddPackage(lotFullPath);

            //add objects from scenegraph (primitive need to change to more robust solution later)
            InvalidateObjects();

            architecture = new LotArchitecture();

            architecture.LoadFromPackage(lotPackage);
#if DEBUG
            ArchitecturePreBakeDebug();
#endif 
            architecture.CreateGameObjects(_lotObject);

            Mode = WallsMode.Roof;
            _state = new WorldState(Floor, Mode);
            architecture.UpdateState(_state);
        }

        void ArchitecturePreBakeDebug()
        { // Bisquicks testing playground :D

        }

        public void InvalidateObjects()
        {
            //Clear any objects on the scene
            UnloadLot();

            var contentProvider = ContentProvider.Get();
            // Go through each lot object.
            foreach (var entry in lotPackage.Entries)
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

        void baseInvalidateGroup(LotArchitecture.ArchitectureGameObjectTypes group)
        {
#if NEW_INVALIDATE
            architecture.InvalidateComponent(group);
            return;
#endif
            if (group == LotArchitecture.ArchitectureGameObjectTypes.roof)
            {
                architecture.InvalidateComponent(group);
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
            architecture.CreateGameObjectsOfType(_lotObject, group);
        }

        public void InvalidateFloors()
        {
            baseInvalidateGroup(LotArchitecture.ArchitectureGameObjectTypes.floor);
            InvalidateTerrain();
        }
        public void InvalidateWalls() => baseInvalidateGroup(LotArchitecture.ArchitectureGameObjectTypes.wall);
        void InvalidateTerrain() => baseInvalidateGroup(LotArchitecture.ArchitectureGameObjectTypes.terrain);

        /// <summary>
        /// After changing the <see cref="WorldState"/>, use this method to update the visual
        /// </summary>
        internal void InvalidateState()
        {
            architecture.UpdateState(_state);
            UpdateObjectVisibility(_state.Level);
        }

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

        private void UpdateObjectVisibility(int viewLevel)
        {
            foreach (GameObject obj in _testObjects)
            {
                int level = architecture.GetLevelAt(obj.transform.GetChild(0).localPosition);

                SetObjectVisiblity(obj, level < viewLevel);
            }
        }        
    }
}
