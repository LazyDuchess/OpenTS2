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
        public List<TSObject> Objects
        {
            get
            {
                return _objectByGUID.Values.ToList();
            }
        }

        Dictionary<uint, TSObject> _objectByGUID = new Dictionary<uint, TSObject>();
        readonly ContentProvider _provider;

        public ObjectManager(ContentProvider provider)
        {
            s_instance = this;
            _provider = provider;
        }

        public void Initialize()
        {
            _objectByGUID = new Dictionary<uint, TSObject>();
            var objectList = _provider.GetAssetsOfType<ObjectDefinitionAsset>(TypeIDs.OBJD); 
            foreach(ObjectDefinitionAsset element in objectList)
            {
                RegisterObject(element);
            }
            //Listeners could be useful in the future or for IDE stuff but not for now.
            /*
            var listener = new AssetListener<ObjectDefinitionAsset>(TypeIDs.OBJD);
            listener.Attach(provider);
            listener.OnUpdateEventHandler += InternalRegister;
            listener.OnRemoveEventHandler += InternalRemove;*/
        }

        TSObject RegisterObject(ObjectDefinitionAsset objd)
        {
            var tsObject = new TSObject(objd);
            _objectByGUID[objd.GUID] = tsObject;
            return tsObject;
        }
    }
}
