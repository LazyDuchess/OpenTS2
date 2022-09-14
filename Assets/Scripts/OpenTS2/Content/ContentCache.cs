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

        public ContentCache()
        {
            _cache = new Dictionary<ResourceKey, WeakReference>();
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Remove(ResourceKey key)
        {
            _cache.Remove(key);
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
        /// Gets a cached object, or caches it if it isn't already.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="objectFactory">Factory function to run if the object is not cached.</param>
        /// <returns>A strong reference to the cached object.</returns>
        public AbstractAsset GetOrAdd(ResourceKey key, Func<ResourceKey, AbstractAsset> objectFactory)
        {
            return GetOrAddInternal(key, objectFactory).Target as AbstractAsset;
        }

        public WeakReference GetWeakReference(ResourceKey key)
        {
            if (_cache.ContainsKey(key))
                return _cache[key];
            return null;
        }
    }
}