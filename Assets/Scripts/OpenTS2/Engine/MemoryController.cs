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
        public static MemoryController Singleton => s_singleton;
        static MemoryController s_singleton = null;
        public struct RemovalInfo
        {
            public UnityEngine.Object[] UnmanagedResources;
            public CacheKey Key;
            public RemovalInfo(CacheKey key, UnityEngine.Object[] unmanagedResources)
            {
                Key = key;
                UnmanagedResources = unmanagedResources;
            }
        }
        private static List<RemovalInfo> MarkedForRemoval = new List<RemovalInfo>();

        private void Awake()
        {
            if (s_singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            s_singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void MarkForRemoval(RemovalInfo info)
        {
            lock(MarkedForRemoval)
                MarkedForRemoval.Add(info);
        }
        private void Update()
        {
            lock (MarkedForRemoval)
            {
                foreach (var removal in MarkedForRemoval)
                {
                    foreach (var unmanagedResource in removal.UnmanagedResources)
                    {
                        unmanagedResource.Free();
                    }
                    var cache = ContentProvider.Get().Cache;
                    if (cache.Cache.TryGetValue(removal.Key, out WeakReference _))
                    {
                        cache.Cache.Remove(removal.Key);
                    }
                }
                MarkedForRemoval.Clear();
            }
        }
    }
}
