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
    public abstract class AssetListener<T> : ContentListener where T : AbstractAsset
    {
        public abstract uint[] Types { get; }
        public override sealed void OnUpdate(ResourceKey key)
        {
            if (!Types.Contains(key.TypeID))
                return;
            var contentAsset = ContentManager.Provider.GetAsset<T>(key);
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
            var allEntries = provider.ResourceMap.Where(x => Types.Contains(x.Key.TypeID)).ToDictionary(x => x.Key, x => x.Value).Values.ToList();
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
        public abstract void AssetUpdated(T asset);
        /// <summary>
        /// Called when an asset gets deleted.
        /// </summary>
        /// <param name="globalTGI">TGI of deleted asset.</param>
        public abstract void AssetDeleted(ResourceKey globalTGI);
    }
}
