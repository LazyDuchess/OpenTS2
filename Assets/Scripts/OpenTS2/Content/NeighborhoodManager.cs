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
using OpenTS2.Engine;

namespace OpenTS2.Content
{
    public class NeighborhoodManager
    {
        public static NeighborhoodManager Instance { get; private set; }
        public Dictionary<uint, string> NeighborhoodObjects { get; private set; }
        public List<Neighborhood> Neighborhoods { get; private set; }

        public Neighborhood CurrentNeighborhood = null;
        public NeighborhoodManager()
        {
            Instance = this;
            Core.OnFinishedLoading += OnFinishedLoading;
        }

        private void OnFinishedLoading()
        {
            Neighborhoods = new List<Neighborhood>();
            NeighborhoodObjects = new Dictionary<uint, string>();
            var contentManager = ContentManager.Instance;
            var allInfos = contentManager.GetAssetsOfType<NeighborhoodInfoAsset>(TypeIDs.NHOOD_INFO);
            foreach (var ninfo in allInfos)
            {
                var nhood = new Neighborhood(ninfo);
                Neighborhoods.Add(nhood);
            }

            // Create a mapping of GUID -> cres files for neighborhood objects.
            var hoodObjects = contentManager.GetAssetsOfType<NeighborhoodObjectAsset>(TypeIDs.NHOOD_OBJECT);
            foreach (var objectAsset in hoodObjects)
            {
                NeighborhoodObjects[objectAsset.Guid] = objectAsset.ModelName;
            }
        }

        public void LeaveNeighborhood()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            CursorController.Cursor = CursorController.CursorType.Hourglass;
            ContentManager.Instance.Changes.SaveChanges();
            if (CurrentNeighborhood != null)
                ContentLoading.UnloadNeighborhoodContentSync();
            CurrentNeighborhood = null;
            SceneManager.LoadScene("Startup");
            CursorController.Cursor = CursorController.CursorType.Default;
        }

        public void EnterNeighborhood(Neighborhood neighborhood)
        {
            CursorController.Cursor = CursorController.CursorType.Hourglass;
            ContentManager.Instance.Changes.SaveChanges();
            if (CurrentNeighborhood != null)
                ContentLoading.UnloadNeighborhoodContentSync();
            ContentLoading.LoadNeighborhoodContentSync(neighborhood);
            CurrentNeighborhood = neighborhood;
            SceneManager.LoadScene("Neighborhood");
            CursorController.Cursor = CursorController.CursorType.Default;
        }
    }
}
