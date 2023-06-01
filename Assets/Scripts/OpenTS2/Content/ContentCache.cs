/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenTS2.Content
{
    public class CacheKey
    {
        public DBPFFile File = null;
        public ResourceKey TGI = default(ResourceKey);
        private readonly ContentProvider _contentProvider;

        public CacheKey(ResourceKey tgi, DBPFFile package = null, ContentProvider contentProvider = null)
        {
            this.TGI = tgi;
            this.File = package;
            if (contentProvider == null)
            {
                contentProvider = ContentProvider.Get();
            }
            this._contentProvider = contentProvider;
            if (package == null && this._contentProvider != null)
            {
                this.File = this._contentProvider.GetEntry(this.TGI)?.Package;
            }
            if (this.File != null)
            {
                this.TGI = this.TGI.GlobalGroupID(this.File.GroupID);
            }
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + TGI.InstanceID.GetHashCode();
                hash = hash * 23 + TGI.InstanceHigh.GetHashCode();
                hash = hash * 23 + TGI.TypeID.GetHashCode();
                hash = hash * 23 + TGI.GroupID.GetHashCode();
                if (File != null)
                    hash = hash * 23 + (int)FileUtils.GroupHash(File.FilePath);
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CacheKey);
        }

        public bool Equals(CacheKey obj)
        {
            return (TGI.InstanceHigh == obj.TGI.InstanceHigh && TGI.InstanceID == obj.TGI.InstanceID && TGI.GroupID == obj.TGI.GroupID && TGI.TypeID == obj.TGI.TypeID && obj.File == File);
        }
    }

    /// <summary>
    /// Manages caching for game content.
    /// </summary>
    public class ContentCache
    {
        // Dictionary to contain the temporary cache.
        public Dictionary<CacheKey, WeakReference> Cache;
        public ContentProvider Provider;

        public ContentCache(ContentProvider provider)
        {
            Provider = provider;
            Cache = new Dictionary<CacheKey, WeakReference>();
        }

        public void Clear()
        {
            lock(Cache)
                Cache.Clear();
        }

        public void Remove(ResourceKey key, DBPFFile package)
        {
            Remove(new CacheKey(key, package, Provider));
        }

        public void Remove(ResourceKey key)
        {
            Remove(new CacheKey(key, null, Provider));
        }

        public void Remove(CacheKey key)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(key))
                    Cache.Remove(key);
            }
        }

        WeakReference GetOrAddInternal(CacheKey key, Func<CacheKey, AbstractAsset> objectFactory)
        {
            lock (Cache)
            {
                if (Cache.TryGetValue(key, out WeakReference result))
                {
                    if (result.Target != null && result.IsAlive && !(result.Target as AbstractAsset).Disposed)
                    {
                        return result;
                    }
                    else
                    {
                        var asset = objectFactory(key);
                        if (asset == null)
                            return null;
                        asset.OnDispose += () => { Remove(key); };
                        result = new WeakReference(asset);
                        Cache[key] = result;
                        return result;
                    }
                }
                else
                {
                    var asset = objectFactory(key);
                    if (asset == null)
                        return null;
                    asset.OnDispose += () => { Remove(key); };
                    result = new WeakReference(asset);
                    Cache[key] = result;
                    return result;
                }
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
            return GetOrAdd(new CacheKey(key, package, Provider), objectFactory);
        }

        /// <summary>
        /// Gets a cached object, or caches it if it isn't already.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="objectFactory">Factory function to run if the object is not cached.</param>
        /// <returns>A strong reference to the cached object.</returns>
        public AbstractAsset GetOrAdd(ResourceKey key, Func<CacheKey, AbstractAsset> objectFactory)
        {
            return GetOrAdd(new CacheKey(key, null, Provider), objectFactory);
        }

        /// <summary>
        /// Gets a cached object, or caches it if it isn't already.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="objectFactory">Factory function to run if the object is not cached.</param>
        /// <returns>A strong reference to the cached object.</returns>
        public AbstractAsset GetOrAdd(CacheKey key, Func<CacheKey, AbstractAsset> objectFactory)
        {
            var result = GetOrAddInternal(key, objectFactory);
            if (result == null)
                return null;
            return result.Target as AbstractAsset;
        }

        public WeakReference GetWeakReference(ResourceKey key, DBPFFile package)
        {
            return GetWeakReference(new CacheKey(key, package, Provider));
        }

        public WeakReference GetWeakReference(ResourceKey key)
        {
            return GetWeakReference(new CacheKey(key, null, Provider));
        }

        public WeakReference GetWeakReference(CacheKey key)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(key))
                    return Cache[key];
                return null;
            }
        }

        public void RemoveAllForPackage(DBPFFile package)
        {
            lock (Cache)
                Cache = Cache.Where(cache => cache.Key.File != package).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}