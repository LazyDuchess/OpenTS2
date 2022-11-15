using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.Listeners
{
    /// <summary>
    /// Listens for asset changes of a specific type.
    /// </summary>
    /// <typeparam name="T">Asset type.</typeparam>
    public class AssetListener<T> : ContentListener where T : AbstractAsset
    {
        readonly uint[] _types;
        public Action<T> OnUpdateEventHandler;
        public Action<ResourceKey> OnRemoveEventHandler;

        /// <summary>
        /// Creates a new AssetListener.
        /// </summary>
        /// <param name="Types">Type ID to listen to.</param>
        public AssetListener(uint Type)
        {
            this._types = new uint[] { Type };
        }

        /// <summary>
        /// Creates a new AssetListener.
        /// </summary>
        /// <param name="Types">Array of Type IDs to listen to.</param>
        public AssetListener(uint[] Types)
        {
            this._types = Types;
        }

        public override sealed void OnUpdate(ResourceKey key)
        {
            if (!_types.Contains(key.TypeID))
                return;
            var contentManager = ContentManager.Get();
            var contentAsset = contentManager.Provider.GetAsset<T>(key);
            if (contentAsset == null)
                AssetDeleted(key);
            else
                AssetUpdated(contentAsset);
        }
        /// <summary>
        /// Start tracking changes from a contentprovider.
        /// </summary>
        /// <param name="provider">ContentProvider to track changes from.</param>
        public override void Attach(ContentProvider provider)
        {
            base.Attach(provider);
            var allEntries = provider.ResourceMap.Where(x => _types.Contains(x.Key.TypeID)).ToDictionary(x => x.Key, x => x.Value).Values.ToList();
            var allAssets = new List<T>();
            foreach(var element in allEntries)
            {
                var ast = element.GetAsset<T>();
                if (ast != null)
                    allAssets.Add(ast);
            }
            Initialize(allAssets);
        }
        void Initialize(List<T> entries)
        {
            //Load all entries
            foreach (var element in entries)
                AssetUpdated(element);
        }
        /// <summary>
        /// Called when an asset gets updated.
        /// </summary>
        /// <param name="asset">Updated asset.</param>
        public virtual void AssetUpdated(T asset)
        {
            OnUpdateEventHandler?.Invoke(asset);
        }
        /// <summary>
        /// Called when an asset gets deleted.
        /// </summary>
        /// <param name="globalTGI">TGI of deleted asset.</param>
        public virtual void AssetDeleted(ResourceKey globalTGI)
        {
            OnRemoveEventHandler?.Invoke(globalTGI);
        }
    }
}
