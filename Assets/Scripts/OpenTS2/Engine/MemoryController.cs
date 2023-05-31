using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine
{
    /// <summary>
    /// Manages clean up of resources that have been garbage collected.
    /// </summary>
    public class MemoryController : MonoBehaviour
    {
        private ContentCache _cache;
        private void Awake()
        {
            _cache = ContentProvider.Get().Cache;
        }
        private void Update()
        {
            var markedForRemoval = new List<CacheKey>();
            foreach(var item in _cache.Cache)
            {
                if (!item.Value.WeakRef.IsAlive)
                {
                    foreach (var unmanagedObject in item.Value.UnmanagedResources)
                        unmanagedObject.Free();
                    markedForRemoval.Add(item.Key);
                }
            }
            foreach(var removal in markedForRemoval)
            {
                _cache.Cache.Remove(removal);
            }
        }
    }
}
