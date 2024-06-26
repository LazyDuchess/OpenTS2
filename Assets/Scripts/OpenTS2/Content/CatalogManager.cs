using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System.Collections.Generic;
using System.Linq;

namespace OpenTS2.Content
{
    public class CatalogManager
    {
        public static CatalogManager Instance { get; private set; }
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
        Dictionary<uint, CatalogRoofAsset> _roofByGUID = new Dictionary<uint, CatalogRoofAsset>();
        readonly ContentManager _manager;

        public CatalogManager()
        {
            Instance = this;
            _manager = ContentManager.Instance;
        }

        public void Initialize()
        {
            _entryByGUID = new Dictionary<uint, CatalogObjectAsset>();
            _fenceByGUID = new Dictionary<uint, CatalogFenceAsset>();
            _roofByGUID = new Dictionary<uint, CatalogRoofAsset>();

            var objectList = _manager.GetAssetsOfType<CatalogObjectAsset>(TypeIDs.CATALOG_OBJECT);
            foreach (CatalogObjectAsset element in objectList)
            {
                RegisterObject(element);
            }

            var fenceList = _manager.GetAssetsOfType<CatalogFenceAsset>(TypeIDs.CATALOG_FENCE);
            foreach (CatalogFenceAsset element in fenceList)
            {
                RegisterFence(element);
            }

            var roofList = _manager.GetAssetsOfType<CatalogRoofAsset>(TypeIDs.CATALOG_ROOF);
            foreach (CatalogRoofAsset element in roofList)
            {
                RegisterRoof(element);
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

        private void RegisterRoof(CatalogRoofAsset catObj)
        {
            // TODO: Follow string set for localization.

            _roofByGUID[catObj.Guid] = catObj;
        }

        public CatalogRoofAsset GetRoofById(uint guid)
        {
            if (_roofByGUID.TryGetValue(guid, out var obj))
            {
                return obj;
            }

            return null;
        }
    }
}
