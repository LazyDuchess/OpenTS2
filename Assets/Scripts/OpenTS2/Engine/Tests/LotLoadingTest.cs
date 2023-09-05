using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.Lot;
using OpenTS2.Scenes.Lot.State;
using UnityEditor;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class LotLoadingTest : MonoBehaviour
    {
        public string NeighborhoodPrefix = "N001";
        public int LotID = 82;

        public int Floor = 5;
        public WallsMode Mode = WallsMode.Roof;

        public int BaseFloor => _architecture?.BaseFloor ?? 0;
        public int MaxFloor => _architecture?.FloorPatterns.Depth ?? 1;

        private WorldState _state = new WorldState(5, WallsMode.Roof);

        private string _nhood;
        private int _lotId;

        private List<GameObject> _lotObject = new List<GameObject>();
        private List<GameObject> _testObjects = new List<GameObject>();
        private LotArchitecture _architecture;

        private void UnloadLot()
        {
            foreach (var obj in _lotObject)
            {
                Destroy(obj);
            }

            _lotObject.Clear();
            _testObjects.Clear();
        }

        private void Start()
        {
            // Load effects.
            EffectsManager.Get().Initialize();

            ContentLoading.LoadGameContentSync();

            CatalogManager.Get().Initialize();

            LoadLot(NeighborhoodPrefix, LotID);
        }

        public void Changed()
        {
            if (NeighborhoodPrefix != _nhood)
            {
                StartCoroutine(ReloadLot());
            }
            else if (LotID != _lotId)
            {
                StartCoroutine(ReloadLot());
            }

            WorldState state = new WorldState(Floor, Mode);

            if (!state.Equals(_state))
            {
                _architecture.UpdateState(state);
                UpdateObjectVisibility(state.Level);

                _state = state;
            }
        }

        System.Collections.IEnumerator ReloadLot()
        {
            yield return new WaitForFixedUpdate();

            if (NeighborhoodPrefix != _nhood || LotID != _lotId)
            {
                UnloadLot();
                LoadLot(NeighborhoodPrefix, LotID);
            }
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
                int level = _architecture.GetLevelAt(obj.transform.GetChild(0).localPosition);

                SetObjectVisiblity(obj, level < viewLevel);
            }
        }

        private void LoadLot(string neighborhoodPrefix, int id)
        {
            _nhood = neighborhoodPrefix;
            _lotId = id;

            var contentProvider = ContentProvider.Get();

            var lotsFolderPath = Path.Combine(Filesystem.GetUserPath(), $"Neighborhoods/{NeighborhoodPrefix}/Lots");
            var lotFilename = $"{NeighborhoodPrefix}_Lot{LotID}.package";
            var lotFullPath = Path.Combine(lotsFolderPath, lotFilename);

            if (!File.Exists(lotFullPath))
            {
                return;
            }

            var lotPackage = contentProvider.AddPackage(lotFullPath);

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

            _architecture = new LotArchitecture();

            _architecture.LoadFromPackage(lotPackage);
            _architecture.CreateGameObjects(_lotObject);

            Floor = MaxFloor + BaseFloor;
            Mode = WallsMode.Roof;
            _state = new WorldState(Floor, Mode);
            _architecture.UpdateState(_state);
        }
    }

    [CustomEditor(typeof(LotLoadingTest))]
    public class LotLoadingTestEditor : Editor
    {
        private string _cachedNhood = "";
        private List<string> NhoodNames = new List<string>() { "N001" };
        private List<int> LotIds = new List<int>() { 82 };

        public LotLoadingTestEditor()
        {
            PopulateNhoodList();
        }

        private void PopulateNhoodList()
        {
            NhoodNames.Clear();

            var nhoodFolderPath = Path.Combine(Filesystem.GetUserPath(), $"Neighborhoods");

            foreach (var nhood in Directory.GetDirectories(nhoodFolderPath))
            {
                string filename = Path.GetFileName(nhood);

                if (filename.Length == 4)
                {
                    NhoodNames.Add(filename);
                }
            }
        }

        private void PopulateLotList(string nhood)
        {
            _cachedNhood = nhood;
            var lotsFolderPath = Path.Combine(Filesystem.GetUserPath(), $"Neighborhoods/{nhood}/Lots");

            LotIds.Clear();

            try
            {
                foreach (string file in Directory.GetFiles(lotsFolderPath))
                {
                    if (file.EndsWith(".package"))
                    {
                        int tIndex = file.LastIndexOf('t');
                        int end = file.Length - ".package".Length;
                        string lotIdProbably = file.Substring(tIndex + 1, end - (tIndex + 1));

                        if (int.TryParse(lotIdProbably, out int lotId))
                        {
                            LotIds.Add(lotId);
                        }
                    }
                }
            }
            catch
            {

            }

            LotIds.Sort();
        }

        public override void OnInspectorGUI()
        {
            var test = target as LotLoadingTest;

            Main.Initialize();

            string[] noptions = NhoodNames.ToArray();
            int nindex = EditorGUILayout.Popup("Neighborhood", NhoodNames.IndexOf(test.NeighborhoodPrefix), noptions);
            if (nindex > -1)
            {
                test.NeighborhoodPrefix = NhoodNames[nindex];
            }

            if (test.NeighborhoodPrefix != _cachedNhood)
            {
                PopulateLotList(test.NeighborhoodPrefix);
            }

            string[] options = LotIds.Select(x => "Lot " + x.ToString()).ToArray();
            int index = EditorGUILayout.Popup("Lot ID", LotIds.IndexOf(test.LotID), options);
            if (index > -1)
            {
                test.LotID = LotIds[index];
            }

            WallsMode walls = (WallsMode)EditorGUILayout.EnumPopup("Walls", test.Mode);
            test.Mode = walls;

            int floor = EditorGUILayout.IntSlider(test.Floor, 1, test.MaxFloor + test.BaseFloor);
            test.Floor = floor;

            test.Changed();
        }
    }
}