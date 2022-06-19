/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Common;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenTS2.Content
{
    /// <summary>
    /// Manages caching for game content.
    /// </summary>
    public class ContentCache
    {
        // Dictionary to contain the temporary cache.
        Dictionary<ResourceKey, WeakReference> _cache;

        // Dictionary to contain the permanent cache (Locked objects)
        Dictionary<ResourceKey, AbstractAsset> _permacache;

        public ContentCache()
        {
            _cache = new Dictionary<ResourceKey, WeakReference>();
        }

        WeakReference GetOrAddInternal(ResourceKey key, Func<ResourceKey, AbstractAsset> objectFactory)
        {
            WeakReference result;
            if (_cache.TryGetValue(key, out result))
            {
                if (result.Target != null && result.IsAlive)
                    return result;
                else
                {
                    result = new WeakReference(objectFactory(key));
                    _cache[key] = result;
                    return result;
                }
            }
            else
            {
                result = new WeakReference(objectFactory(key));
                _cache[key] = result;
                return result;
            }
        }

        /// <summary>
        /// Unlocks all locked objects.
        /// </summary>
        public void ReleaseAllLocks()
        {
            _permacache.Clear();
        }

        /// <summary>
        /// Unlocks a cached object so that it will get garbage collected if necessary.
        /// </summary>
        /// <param name="key">Object key.</param>
        public void Unlock(ResourceKey key)
        {
            _permacache[key] = null;
        }

        /// <summary>
        /// Gets a cached object, or caches it if it isn't already, then locks it so it won't get garbage collected.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="objectFactory">Factory function to run if the object is not cached.</param>
        /// <returns>A strong reference to the cached object.</returns>
        public AbstractAsset GetOrAddLocked(ResourceKey key, Func<ResourceKey, AbstractAsset> objectFactory)
        {
            var result = GetOrAdd(key, objectFactory);
            _permacache[key] = result;
            return result;
        }

        /// <summary>
        /// Gets a cached object, or caches it if it isn't already.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="objectFactory">Factory function to run if the object is not cached.</param>
        /// <returns>A strong reference to the cached object.</returns>
        public AbstractAsset GetOrAdd(ResourceKey key, Func<ResourceKey, AbstractAsset> objectFactory)
        {
            return GetOrAddInternal(key, objectFactory).Target as AbstractAsset;
        }

        /// <summary>
        /// Adds a new asset to the cache, permanently in memory until it is serialized to the device.
        /// Useful for modifying/creating assets without losing them to garbage collection.
        /// </summary>
        /// <param name="key">Asset key.</param>
        /// <param name="asset">Asset.</param>
        public void AddLocked(ResourceKey key, AbstractAsset asset)
        {
            _permacache[key] = asset;
            _cache[key] = new WeakReference(asset);
        }
    }
}