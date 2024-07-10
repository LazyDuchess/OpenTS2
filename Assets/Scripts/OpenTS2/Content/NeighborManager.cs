using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Content
{
    public class NeighborManager : MonoBehaviour
    {
        public static NeighborManager Instance { get; private set; }
        public Dictionary<short, NeighborAsset> NeighborById;
        private ContentManager _contentManager;

        public void Initialize()
        {
            _contentManager = ContentManager.Instance;
            NeighborById = new Dictionary<short, NeighborAsset>();
            var neighbors = _contentManager.GetAssetsOfType<NeighborAsset>(TypeIDs.NEIGHBOR);
            foreach(var neighbor in neighbors)
            {
                NeighborById[neighbor.Id] = neighbor;
            }
        }

        public static void Create()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            var go = new GameObject("Neighbor Manager");
            Instance = go.AddComponent<NeighborManager>();
            Instance.Initialize();
        }
    }
}
