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
        public static List<ObjectDefinitionAsset> Objects
        {
            get
            {
                return _objectByGUID.Values.ToList();
            }
        }

        private static Dictionary<uint, ObjectDefinitionAsset> _objectByGUID = new Dictionary<uint, ObjectDefinitionAsset>();

        private void Awake()
        {
            Core.OnFinishedLoading += OnFinishedLoading;
        }

        private void OnFinishedLoading()
        {
            _objectByGUID = new Dictionary<uint, ObjectDefinitionAsset>();
            var objectList = ContentProvider.Get().GetAssetsOfType<ObjectDefinitionAsset>(TypeIDs.OBJD); 
            foreach(ObjectDefinitionAsset element in objectList)
            {
                RegisterObject(element);
            }
        }

        private void RegisterObject(ObjectDefinitionAsset objd)
        {
            _objectByGUID[objd.GUID] = objd;
        }

        public static ObjectDefinitionAsset GetObjectByGUID(uint guid)
        {
            if (_objectByGUID.TryGetValue(guid, out ObjectDefinitionAsset obj))
                return obj;
            return null;
        }
    }
}
