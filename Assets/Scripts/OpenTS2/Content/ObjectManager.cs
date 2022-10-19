using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.Listeners;
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
            return INSTANCE;
        }
        
        static ObjectManager INSTANCE;
        public List<TSObject> Objects
        {
            get
            {
                return objectByGUID.Values.ToList();
            }
        }

        Dictionary<uint, TSObject> objectByGUID = new Dictionary<uint, TSObject>();
        ContentProvider Provider;

        public ObjectManager(ContentProvider provider)
        {
            INSTANCE = this;
            Provider = provider;
        }

        public void Initialize()
        {
            objectByGUID = new Dictionary<uint, TSObject>();
            var objectList = Provider.GetAssetsOfType<ObjectDefinitionAsset>(TypeIDs.OBJD); 
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
            objectByGUID[objd.guid] = tsObject;
            return tsObject;
        }
    }
}
