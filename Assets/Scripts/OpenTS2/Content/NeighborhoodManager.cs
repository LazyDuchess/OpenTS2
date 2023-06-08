using OpenTS2.Content.DBPF;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenTS2.Content
{
    public static class NeighborhoodManager
    {
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
        }

        public static void EnterNeighborhood(Neighborhood neighborhood)
        {
            CurrentNeighborhood = neighborhood;
            SceneManager.LoadScene("Neighborhood");
        }
    }
}
