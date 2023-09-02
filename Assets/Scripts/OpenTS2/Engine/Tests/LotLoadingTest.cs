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
using OpenTS2.Scenes.Lot.Roof;
using UnityEditor;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class LotLoadingTest : MonoBehaviour
    {
        public string NeighborhoodPrefix = "N001";
        public int LotID = 82;

        private string _nhood;
        private int _lotId;

        private List<GameObject> _lotObject = new List<GameObject>();

        private void UnloadLot()
        {
            foreach (var obj in _lotObject)
            {
                Destroy(obj);
            }
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
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var wallStyles = lotPackage.Entries.First(x => x.GlobalTGI.TypeID == TypeIDs.LOT_STRINGMAP && x.GlobalTGI.InstanceID == 13).GetAsset<StringMapAsset>();
            var floorStyles = lotPackage.Entries.First(x => x.GlobalTGI.TypeID == TypeIDs.LOT_STRINGMAP && x.GlobalTGI.InstanceID == 14).GetAsset<StringMapAsset>();

            var wallGraphs = lotPackage.Entries.Where(x => x.GlobalTGI.TypeID == TypeIDs.LOT_WALLGRAPH).Select(x => x.GetAsset<WallGraphAsset>()).ToList();

            var arry3 = lotPackage.Entries.Where(x => x.GlobalTGI.TypeID == TypeIDs.LOT_3ARY).Select(x => x.GetAsset()).ToList();

            // Shared

            var wallLayer = lotPackage.GetAssetByTGI<WallLayerAsset>(new ResourceKey(4, uint.MaxValue, TypeIDs.LOT_WALLLAYER));
            var wallGraph = lotPackage.GetAssetByTGI<WallGraphAsset>(new ResourceKey(0x18, uint.MaxValue, TypeIDs.LOT_WALLGRAPH));
            var floorElevation = lotPackage.GetAssetByTGI<_3DArrayAsset<float>>(new ResourceKey(1, uint.MaxValue, TypeIDs.LOT_3ARY));
            var pool = lotPackage.GetAssetByTGI<PoolAsset>(new ResourceKey(0, uint.MaxValue, TypeIDs.LOT_POOL));
            var roof = lotPackage.GetAssetByTGI<RoofAsset>(new ResourceKey(0, uint.MaxValue, TypeIDs.LOT_ROOF));

            var roofs = new RoofCollection(roof.Entries, floorElevation, wallGraph.BaseFloor);

            // Roofs

            var roofObj = new GameObject("roof", typeof(LotRoofComponent));
            roofObj.GetComponent<LotRoofComponent>().CreateFromLotAssets(roof.Entries.FirstOrDefault().Pattern, roofs);
            _lotObject.Add(roofObj);

            // Walls

            var wallGraphR = wallGraph;
            var fencePosts = lotPackage.GetAssetByTGI<FencePostLayerAsset>(new ResourceKey(0x11, uint.MaxValue, TypeIDs.LOT_FENCEPOST));

            var wall = new GameObject("wall", typeof(LotWallComponent));
            wall.GetComponent<LotWallComponent>().CreateFromLotAssets(wallStyles, wallLayer, wallGraphR, fencePosts, floorElevation, roofs);
            _lotObject.Add(wall);

            // Floors

            var floorPatterns = lotPackage.GetAssetByTGI<_3DArrayAsset<Vector4<ushort>>>(new ResourceKey(0, uint.MaxValue, TypeIDs.LOT_3ARY));

            var floor = new GameObject("floor", typeof(LotFloorComponent));
            floor.GetComponent<LotFloorComponent>().CreateFromLotAssets(floorStyles, floorPatterns, floorElevation, wallGraph.BaseFloor);
            // Small bias until the floor is cut out of the terrain.
            floor.transform.position = new Vector3(0, 0.001f, 0);
            _lotObject.Add(floor);

            // Terrain

            var terrainTextures = lotPackage.Entries.First(x => x.GlobalTGI.TypeID == TypeIDs.LOT_TEXTURES).GetAsset<LotTexturesAsset>();
            var terrainData = lotPackage.Entries.Where(x => x.GlobalTGI.TypeID == TypeIDs.LOT_TERRAIN).Select(x => x.GetAsset()).ToList();

            var waterHeightmap = (_2DArrayAsset<float>)terrainData.First(x => x is IArrayAsset array && array.ArrayType() == typeof(float));
            var blend = terrainData.Where(x => x is IArrayAsset array && array.ArrayType() == typeof(byte))
                .OrderBy(x => x.GlobalTGI.InstanceID)
                .Select(x => (_2DArrayAsset<byte>)x)
                .Take(terrainTextures.BlendTextures.Length)
                .ToArray();

            var terrain = new GameObject("terrain", typeof(LotTerrainComponent));
            terrain.GetComponent<LotTerrainComponent>().CreateFromTerrainAssets(terrainTextures, floorElevation, blend, waterHeightmap, wallGraph.BaseFloor);
            _lotObject.Add(terrain);
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

            test.Changed();
        }
    }
}