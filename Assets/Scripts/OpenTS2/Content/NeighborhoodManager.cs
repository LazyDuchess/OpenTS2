using OpenTS2.Audio;
using OpenTS2.Content.DBPF;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using OpenTS2.Rendering;

namespace OpenTS2.Content
{
    public static class NeighborhoodManager
    {
        public static Dictionary<uint, string> NeighborhoodObjects = new Dictionary<uint, string>();

        public static Neighborhood CurrentNeighborhood = null;
        public static List<Neighborhood> Neighborhoods => _neighborHoods;
        static List<Neighborhood> _neighborHoods = new List<Neighborhood>();
        public static void Initialize()
        {
            _neighborHoods.Clear();
            var contentProvider = ContentProvider.Get();
            var allInfos = contentProvider.GetAssetsOfType<NeighborhoodInfoAsset>(TypeIDs.NHOOD_INFO);
            foreach(var ninfo in allInfos)
            {
                var nhood = new Neighborhood(ninfo);
                _neighborHoods.Add(nhood);
            }

            // Create a mapping of GUID -> cres files for neighborhood objects.
            var hoodObjects = contentProvider.GetAssetsOfType<NeighborhoodObjectAsset>(TypeIDs.NHOOD_OBJECT);
            foreach (var objectAsset in hoodObjects)
            {
                NeighborhoodObjects[objectAsset.Guid] = objectAsset.ModelName;
            }
        }

        public static void LeaveNeighborhood()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            CursorController.Cursor = CursorController.CursorType.Hourglass;
            ContentProvider.Get().Changes.SaveChanges();
            if (CurrentNeighborhood != null)
                ContentLoading.UnloadNeighborhoodContentSync();
            CurrentNeighborhood = null;
            SceneManager.LoadScene("Startup");
            CursorController.Cursor = CursorController.CursorType.Default;
        }

        public static void EnterNeighborhood(Neighborhood neighborhood)
        {
            CursorController.Cursor = CursorController.CursorType.Hourglass;
            ContentProvider.Get().Changes.SaveChanges();
            if (CurrentNeighborhood != null)
                ContentLoading.UnloadNeighborhoodContentSync();
            ContentLoading.LoadNeighborhoodContentSync(neighborhood);
            MusicController.FadeOutMusic();
            CurrentNeighborhood = neighborhood;
            SceneManager.LoadScene("Neighborhood");
            CursorController.Cursor = CursorController.CursorType.Default;
        }
    }
}
