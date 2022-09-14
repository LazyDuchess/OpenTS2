/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenTS2.Content
{
    public class CacheKey
    {
        public DBPFFile file = null;
        public ResourceKey tgi = ResourceKey.Default;

        public CacheKey(ResourceKey tgi)
        {
            this.tgi = tgi;
        }

        public CacheKey(ResourceKey tgi, DBPFFile package)
        {
            this.tgi = tgi;
            this.file = package;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + tgi.InstanceID.GetHashCode();
                hash = hash * 23 + tgi.InstanceHigh.GetHashCode();
                hash = hash * 23 + tgi.TypeID.GetHashCode();
                hash = hash * 23 + tgi.GroupID.GetHashCode();
                if (file != null)
                    hash = hash * 23 + (int)FileUtils.GroupHash(file.FilePath);
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CacheKey);
        }

        public bool Equals(CacheKey obj)
        {
            return (tgi.InstanceHigh == obj.tgi.InstanceHigh && tgi.InstanceID == obj.tgi.InstanceID && tgi.GroupID == obj.tgi.GroupID && tgi.TypeID == obj.tgi.TypeID && obj.file == file);
        }
    }
    /// <summary>
    /// Manages caching for game content.
    /// </summary>
    public class ContentCache
    {
        // Dictionary to contain the temporary cache.
        Dictionary<CacheKey, WeakReference> _cache;

        public ContentCache()
        {
            _cache = new Dictionary<CacheKey, WeakReference>();
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Remove(ResourceKey key, DBPFFile package)
        {
            Remove(new CacheKey(key, package));
        }

        public void Remove(ResourceKey key)
        {
            Remove(new CacheKey(key));
        }

        public void Remove(CacheKey key)
        {
            _cache.Remove(key);
        }

        WeakReference GetOrAddInternal(CacheKey key, Func<CacheKey, AbstractAsset> objectFactory)
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
        /// <param name="package">Package file.</param>
        /// <param name="objectFactory">Factory function to run if the object is not cached.</param>
        /// <returns>A strong reference to the cached object.</returns>
        public AbstractAsset GetOrAdd(ResourceKey key, DBPFFile package, Func<CacheKey, AbstractAsset> objectFactory)
        {
            return GetOrAdd(new CacheKey(key, package), objectFactory);
        }

        /// <summary>
        /// Gets a cached object, or caches it if it isn't already.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="objectFactory">Factory function to run if the object is not cached.</param>
        /// <returns>A strong reference to the cached object.</returns>
        public AbstractAsset GetOrAdd(ResourceKey key, Func<CacheKey, AbstractAsset> objectFactory)
        {
            return GetOrAdd(new CacheKey(key), objectFactory);
        }

        /// <summary>
        /// Gets a cached object, or caches it if it isn't already.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="objectFactory">Factory function to run if the object is not cached.</param>
        /// <returns>A strong reference to the cached object.</returns>
        public AbstractAsset GetOrAdd(CacheKey key, Func<CacheKey, AbstractAsset> objectFactory)
        {
            return GetOrAddInternal(key, objectFactory).Target as AbstractAsset;
        }

        public WeakReference GetWeakReference(ResourceKey key, DBPFFile package)
        {
            return GetWeakReference(new CacheKey(key, package));
        }

        public WeakReference GetWeakReference(ResourceKey key)
        {
            return GetWeakReference(new CacheKey(key));
        }

        public WeakReference GetWeakReference(CacheKey key)
        {
            if (_cache.ContainsKey(key))
                return _cache[key];
            return null;
        }
    }
}