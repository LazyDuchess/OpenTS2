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
    /// Listens for changes to resources in contentproviders.
    /// </summary>
    public abstract class ContentListener
    {
        public virtual void DeAttach(ContentProvider provider)
        {
            provider.OnContentChangedEventHandler -= OnUpdate;
        }
        public virtual void Attach(ContentProvider provider)
        {
            provider.OnContentChangedEventHandler += OnUpdate;
        }

        //Resource was updated.
        public abstract void OnUpdate(ResourceKey key);
    }
}
