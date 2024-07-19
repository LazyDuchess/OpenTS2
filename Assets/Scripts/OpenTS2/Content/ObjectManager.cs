using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Content
{
    public class ObjectManager : MonoBehaviour
    {
        public static ObjectManager Instance { get; private set; }
        public Dictionary<uint, ObjectDefinitionAsset> ObjectByGUID;
        public List<ObjectDefinitionAsset> Objects => ObjectByGUID.Values.ToList();
        private ContentManager _contentManager;

        public void Initialize()
        {
            _contentManager = ContentManager.Instance;
            ObjectByGUID = new Dictionary<uint, ObjectDefinitionAsset>();
            var objects = _contentManager.GetAssetsOfType<ObjectDefinitionAsset>(TypeIDs.OBJD);
            foreach (var objd in objects)
            {
                ObjectByGUID[objd.GUID] = objd;
            }
        }

        public ObjectDefinitionAsset GetObjectByGUID(uint guid)
        {
            if (ObjectByGUID.TryGetValue(guid, out var objd))
                return objd;
            return null;
        }

        public static void Create()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            var go = new GameObject("Object Manager");
            Instance = go.AddComponent<ObjectManager>();
            Instance.Initialize();
        }
    }
}
