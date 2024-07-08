using OpenTS2.Engine.Tests;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Scenes.Lot.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace OpenTS2.Engine.Tests
{
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

            var nhoodFolderPath = Path.Combine(Filesystem.UserDataDirectory, $"Neighborhoods");

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
            var lotsFolderPath = Path.Combine(Filesystem.UserDataDirectory, $"Neighborhoods/{nhood}/Lots");

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

            Core.InitializeCore();

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
