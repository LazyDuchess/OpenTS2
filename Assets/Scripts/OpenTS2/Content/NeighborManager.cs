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
        public List<NeighborAsset> Neighbors => NeighborById.Values.ToList();
        private ContentManager _contentManager;

        public void Initialize()
        {
            var currentNeighborhood = NeighborhoodManager.Instance.CurrentNeighborhood;
            _contentManager = ContentManager.Instance;
            NeighborById = new Dictionary<short, NeighborAsset>();
            var neighborEntries = _contentManager.ResourceMap.Values.Where(x => x.GlobalTGI.TypeID == TypeIDs.NEIGHBOR && x.GlobalTGI.GroupID == currentNeighborhood.GroupID);
            foreach(var neighborEntry in neighborEntries)
            {
                var neighbor = neighborEntry.GetAsset<NeighborAsset>();
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
