using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System.Collections.Generic;
using System.Linq;

namespace OpenTS2.Content
{
    public class CatalogManager
    {
        public static CatalogManager Get()
        {
            return s_instance;
        }

        static CatalogManager s_instance;
        public List<CatalogObjectAsset> Objects
        {
            get
            {
                return _entryByGUID.Values.ToList();
            }
        }

        public List<CatalogFenceAsset> Fences
        {
            get
            {
                return _fenceByGUID.Values.ToList();
            }
        }

        Dictionary<uint, CatalogObjectAsset> _entryByGUID = new Dictionary<uint, CatalogObjectAsset>();
        Dictionary<uint, CatalogFenceAsset> _fenceByGUID = new Dictionary<uint, CatalogFenceAsset>();
        readonly ContentProvider _provider;

        public CatalogManager(ContentProvider provider)
        {
            s_instance = this;
            _provider = provider;
        }

        public void Initialize()
        {
            _entryByGUID = new Dictionary<uint, CatalogObjectAsset>();
            _fenceByGUID = new Dictionary<uint, CatalogFenceAsset>();

            var objectList = _provider.GetAssetsOfType<CatalogObjectAsset>(TypeIDs.CATALOG_OBJECT);
            foreach (CatalogObjectAsset element in objectList)
            {
                RegisterObject(element);
            }

            var fenceList = _provider.GetAssetsOfType<CatalogFenceAsset>(TypeIDs.CATALOG_FENCE);
            foreach (CatalogFenceAsset element in fenceList)
            {
                RegisterFence(element);
            }
        }

        private void RegisterObject(CatalogObjectAsset catObj)
        {
            // TODO: Follow string set for localization.

            _entryByGUID[catObj.Guid] = catObj;
        }

        public CatalogObjectAsset GetEntryById(uint guid)
        {
            if (_entryByGUID.TryGetValue(guid, out var obj))
            {
                return obj;
            }

            return null;
        }

        private void RegisterFence(CatalogFenceAsset catObj)
        {
            // TODO: Follow string set for localization.

            _fenceByGUID[catObj.Guid] = catObj;
        }

        public CatalogFenceAsset GetFenceById(uint guid)
        {
            if (_fenceByGUID.TryGetValue(guid, out var obj))
            {
                return obj;
            }

            return null;
        }
    }
}
