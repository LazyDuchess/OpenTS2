using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class ObjectManager
    {
        public static ObjectManager Get()
        {
            return s_instance;
        }
        
        static ObjectManager s_instance;
        public List<ObjectDefinitionAsset> Objects
        {
            get
            {
                return _objectByGUID.Values.ToList();
            }
        }

        Dictionary<uint, ObjectDefinitionAsset> _objectByGUID = new Dictionary<uint, ObjectDefinitionAsset>();
        readonly ContentProvider _provider;

        public ObjectManager(ContentProvider provider)
        {
            s_instance = this;
            _provider = provider;
        }

        public void Initialize()
        {
            _objectByGUID = new Dictionary<uint, ObjectDefinitionAsset>();
            var objectList = _provider.GetAssetsOfType<ObjectDefinitionAsset>(TypeIDs.OBJD); 
            foreach(ObjectDefinitionAsset element in objectList)
            {
                RegisterObject(element);
            }
        }

        void RegisterObject(ObjectDefinitionAsset objd)
        {
            _objectByGUID[objd.GUID] = objd;
        }

        public ObjectDefinitionAsset GetObjectByGUID(uint guid)
        {
            if (_objectByGUID.TryGetValue(guid, out ObjectDefinitionAsset obj))
                return obj;
            return null;
        }
    }
}
