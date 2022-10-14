using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.Listeners
{
    public abstract class AssetListener<T> : ContentListener where T : AbstractAsset
    {
        public abstract uint[] Types { get; }
        public override sealed void OnUpdate(ResourceKey key)
        {
            if (!Types.Contains(key.TypeID))
                return;
            var contentAsset = ContentManager.Get.Provider.GetAsset<T>(key);
            if (contentAsset == null)
                AssetDeleted(key);
            else
                AssetUpdated(contentAsset);
        }
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
            foreach (var element in entries)
                AssetUpdated(element);
        }
        public abstract void AssetUpdated(T asset);
        public abstract void AssetDeleted(ResourceKey globalTGI);
    }
}
