/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    /// <summary>
    /// Provides access to reading and writing to assets in packages.
    /// </summary>
    public class ContentProvider : IDisposable
    {
        static ContentProvider s_instance;
        public static ContentProvider Get()
        {
            return s_instance;
        }
        public Dictionary<ResourceKey, DBPFEntry> ResourceMap
        {
            get { return _resourceMap; }
        }
        public List<DBPFFile> ContentEntries
        {
            get { return _contentEntries; }
        }

        private readonly Dictionary<ResourceKey, DBPFEntry> _resourceMap = new Dictionary<ResourceKey, DBPFEntry>();
        private readonly List<DBPFFile> _contentEntries = new List<DBPFFile>();
        private readonly Dictionary<uint, DBPFFile> _entryByGroupID = new Dictionary<uint, DBPFFile>();
        private readonly Dictionary<string, DBPFFile> _entryByPath = new Dictionary<string, DBPFFile>();
        private readonly Dictionary<DBPFFile, DBPFFile> _entryByFile = new Dictionary<DBPFFile, DBPFFile>();
        public ContentCache Cache;
        public ContentChanges Changes;
        public Action<ResourceKey> OnContentChangedEventHandler;

        public ContentProvider()
        {
            s_instance = this;
            this.Changes = new ContentChanges(this);
            this.Cache = new ContentCache(this);
        }

        AbstractAsset InternalLoadAsset(CacheKey key)
        {
            if (key.File == null)
                return null;
            return key.File.GetAssetByTGI(key.TGI);
        }

        /// <summary>
        /// Gets all entries of a specific type.
        /// </summary>
        /// <param name="typeID">Type</param>
        /// <returns>List of DBPFEntries of specified type.</returns>
        public List<DBPFEntry> GetEntriesOfType(uint typeID)
        {
            return _resourceMap.Where(map => map.Value.GlobalTGI.TypeID == typeID).ToDictionary(x => x.Key, x => x.Value).Values.ToList();
        }

        /// <summary>
        /// Gets all Assets of a specific Type ID in the Resource Map.
        /// </summary>
        /// <param name="TypeID">Type ID</param>
        /// <returns>List of assets.</returns>
        public List<AbstractAsset> GetAssetsOfType(uint TypeID)
        {
            return GetAssetsOfType<AbstractAsset>(TypeID);
        }

        /// <summary>
        /// Gets all Assets of a specific Type ID in the Resource Map.
        /// </summary>
        /// <param name="TypeID">Type</param>
        /// <typeparam name="T">Type of Asset</typeparam>
        /// <returns>List of assets.</returns>
        public List<T> GetAssetsOfType<T>(uint TypeID) where T : AbstractAsset
        {
            var entryList = GetEntriesOfType(TypeID);
            var assetList = new List<T>();
            foreach(var element in entryList)
            {
                var outputAsset = element.GetAsset<T>();
                if (outputAsset != null)
                    assetList.Add(outputAsset);
            }
            return assetList;
        }

        /// <summary>
        /// Caches an asset into the content system and returns it.
        /// </summary>
        /// <param name="key">Key of the resource.</param>
        /// <returns>The asset.</returns>
        public AbstractAsset GetAsset(CacheKey key)
        {
            return Cache.GetOrAdd(key, InternalLoadAsset);
        }

        /// <summary>
        /// Caches an asset into the content system and returns it.
        /// </summary>
        /// <param name="tgi">TGI of the resource.</param>
        /// <returns>The asset.</returns>
        public AbstractAsset GetAsset(ResourceKey tgi, DBPFFile file)
        {
            return Cache.GetOrAdd(tgi, file, InternalLoadAsset);
        }

        /// <summary>
        /// Caches an asset into the content system and returns it.
        /// </summary>
        /// <param name="tgi">TGI of the resource.</param>
        /// <returns>The asset.</returns>
        public AbstractAsset GetAsset(ResourceKey tgi)
        {
            return Cache.GetOrAdd(tgi, InternalLoadAsset);
        }

        /// <summary>
        /// Caches an asset into the content system and returns it.
        /// </summary>
        /// <typeparam name="T">The Type of asset to return.</typeparam>
        /// <param name="tgi">TGI of the resource.</param>
        /// <returns>The asset.</returns>
        public T GetAsset<T>(ResourceKey tgi) where T : AbstractAsset
        {
            return GetAsset(tgi) as T;
        }

        class AsyncDBPFFile
        {
            public DBPFFile file;
            public string path;
        }

        public void AddPackages(List<string> paths)
        {
            foreach(var element in paths)
            {
                AddPackage(element);
            }
        }

        /// <summary>
        /// Loads a list of packages in parallel.
        /// </summary>
        /// <param name="paths">List of package paths.</param>
        /// <returns></returns>
        public async Task AddPackagesAsync(List<string> paths, LoadProgress loadProgress = null)
        {
            await AddPackagesAsync(paths.ToArray(), loadProgress);
        }

        /// <summary>
        /// Loads an array of packages in parallel.
        /// </summary>
        /// <param name="paths">Array of package paths.</param>
        /// <returns></returns>
        async Task AddPackagesAsync(string[] paths, LoadProgress loadProgress = null)
        {
            var tasks = new List<Task>();
            var packages = new List<AsyncDBPFFile>();
            for(var i=0;i<paths.Length;i++)
            {
                var async = new AsyncDBPFFile
                {
                    path = paths[i]
                };
                packages.Add(async);
                var capturedI = i;
                var fileCount = paths.Length;
                tasks.Add(Task.Run(() => { 
                    async.file = new DBPFFile(async.path);
                    if (loadProgress != null)
                    {
                        var progress = (float)capturedI / fileCount;
                        if (loadProgress.Percentage < progress)
                            loadProgress.Percentage = (float)capturedI / fileCount;
                    }
                }));
            }
            await Task.WhenAll(tasks).ContinueWith((task) =>
            {
                foreach (var element in packages)
                {
                    AddPackage(element.file);
                }
                loadProgress.Percentage = 1f;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Adds a DBPF Package to the filesystem, or returns existing package if already loaded.
        /// </summary>
        /// <param name="path">Path to the package.</param>
        /// <returns>Package.</returns>
        public DBPFFile AddPackage(string path)
        {
            path = Filesystem.GetRealPath(path);
            if (_entryByPath.ContainsKey(path))
                return _entryByPath[path];
            var package = new DBPFFile(path);
            InternalAddPackage(package);
            return package;
        }

        void InternalAddPackage(DBPFFile package)
        {
            package.Provider = this;
            //Resource map goes from first to last, keeps the first resource it finds with the requested TGI.
            _contentEntries.Insert(0, package);
            AddToTopOfResourceMap(package);
            _entryByGroupID[package.GroupID] = package;
            _entryByPath[package.FilePath] = package;
            _entryByFile[package] = package;
        }

        /// <summary>
        /// Gets an entry from the resource map.
        /// </summary>
        /// <param name="key">TGI of the resource.</param>
        /// <returns></returns>
        public DBPFEntry GetEntry(ResourceKey key)
        {
            if (_resourceMap.TryGetValue(key, out DBPFEntry output))
                return output;
            return null;
        }

        /// <summary>
        /// Adds all entries from a package into the resource map, at the top of the stack.
        /// </summary>
        /// <param name="package">Package to add entries from.</param>
        public void AddToTopOfResourceMap(DBPFFile package)
        {
            var entries = package.Entries;
            foreach (var element in entries)
            {
                AddToTopOfResourceMap(element);
            }
        }

        /// <summary>
        /// Adds an entry to the resource map, at the top of the stack.
        /// </summary>
        /// <param name="entry">Entry to add</param>
        public void AddToTopOfResourceMap(DBPFEntry entry)
        {
            _resourceMap[entry.GlobalTGI] = entry;
            OnContentChangedEventHandler?.Invoke(entry.GlobalTGI);
        }

        /// <summary>
        /// Attempts to add all entries from a package into the resource map. Will not work if there are resources with the same TGIs higher up in the stack.
        /// </summary>
        /// <param name="package">Package to add entries from</param>
        public void UpdateOrAddToResourceMap(DBPFFile package)
        {
            var entries = package.Entries;
            foreach (var element in entries)
            {
                UpdateOrAddToResourceMap(element);
            }
        }

        /// <summary>
        /// Attempts to add an entry to the resource map. Will not work if there are resources with the same TGI higher up in the stack.
        /// </summary>
        /// <param name="entry">Entry to add to resource map</param>
        public void UpdateOrAddToResourceMap(DBPFEntry entry)
        {
            if (_resourceMap.TryGetValue(entry.GlobalTGI, out DBPFEntry output))
            {
                if (entry.Package == output.Package)
                {
                    _resourceMap[entry.GlobalTGI] = entry;
                }
                else
                {
                    FindEntryForMap(entry.GlobalTGI);
                }
            }
            else
                AddToTopOfResourceMap(entry);
            OnContentChangedEventHandler?.Invoke(entry.GlobalTGI);
        }

        /// <summary>
        /// Remove all entries from a package from the resource map. Requires all entries to be synchronized.
        /// </summary>
        /// <param name="package">Package to remove from map.</param>
        public void RemoveFromResourceMap(DBPFFile package)
        {
            var entries = package.Entries;
            foreach (var element in entries)
            {
                RemoveFromResourceMap(element);
            }
        }
        /// <summary>
        /// Remove a resource from the resourcemap, by its entry, then update with the next entry.
        /// </summary>
        /// <param name="entry">Entry for resource</param>
        public void RemoveFromResourceMap(DBPFEntry entry)
        {
            RemoveFromResourceMap(entry.GlobalTGI, entry.Package);
        }

        /// <summary>
        /// Remove a resource from the resourcemap, by its global TGI and package file, then update with the next entry.
        /// </summary>
        /// <param name="tgi">TGI</param>
        /// <param name="package">Package</param>
        public void RemoveFromResourceMap(ResourceKey tgi, DBPFFile package)
        {
            if (_resourceMap.TryGetValue(tgi, out DBPFEntry output))
            {
                if (package == output.Package)
                {
                    _resourceMap.Remove(tgi);
                    FindEntryForMap(tgi);
                    OnContentChangedEventHandler?.Invoke(tgi);
                }
            }
        }

        /// <summary>
        /// Find topmost resource for this TGI to add to resource map.
        /// </summary>
        /// <param name="tgi">TGI</param>
        void FindEntryForMap(ResourceKey tgi)
        {
            foreach (var element in _contentEntries)
            {
                var localTGI = tgi.GlobalGroupID(element.GroupID);
                var entryByTGI = element.GetEntryByTGI(localTGI);
                if (entryByTGI != null)
                {
                    _resourceMap[tgi] = entryByTGI;
                    return;
                }
                entryByTGI = element.GetEntryByTGI(tgi);
                if (entryByTGI != null)
                {
                    _resourceMap[tgi] = entryByTGI;
                    return;
                }
            }
        }

        /// <summary>
        /// Adds a DBPF Package to the filesystem. Removes any packages with the same path.
        /// </summary>
        /// <param name="package">DBPFFile to add.</param>
        /// <returns>Content entry for package.</returns>
        public void AddPackage(DBPFFile package)
        {
            if (_entryByPath.ContainsKey(package.FilePath))
            {
                RemovePackage(_entryByPath[package.FilePath]);
            }
            InternalAddPackage(package);
        }

        /// <summary>
        /// Removes a DBPF Package from the filesystem.
        /// </summary>
        /// <param name="package">Content entry for the package to remove.</param>
        public void RemovePackage(DBPFFile package)
        {
            //package.Dispose();
            if (_entryByFile.ContainsKey(package))
            {
                package.Provider = null;
                RemoveFromResourceMap(package);
                Cache.RemoveAllForPackage(package);
                _contentEntries.Remove(package);
                _entryByGroupID.Remove(package.GroupID);
                _entryByPath.Remove(package.FilePath);
                _entryByFile.Remove(package);
            }
        }

        /// <summary>
        /// Gets a DBPF Package by its generated Group ID.
        /// </summary>
        /// <param name="groupID">Group ID.</param>
        /// <returns>Content entry for package.</returns>
        public DBPFFile GetPackageByGroup(uint groupID)
        {
            if (_entryByGroupID.ContainsKey(groupID))
                return _entryByGroupID[groupID];
            else
                return null;
        }

        /// <summary>
        /// Gets a DBPF Package by its filename without extension or path (Generated Group ID)
        /// </summary>
        /// <param name="groupName">Name of the file without extension or path.</param>
        /// <returns>Content entry for package.</returns>
        public DBPFFile GetPackageByGroup(string groupName)
        {
            var groupID = FileUtils.GroupHash(groupName);
            if (_entryByGroupID.ContainsKey(groupID))
                return _entryByGroupID[groupID];
            else
                return null;
        }

        /// <summary>
        /// Gets a DBPF Package by its filesystem path.
        /// </summary>
        /// <param name="path">Filesystem path.</param>
        /// <returns>Content entry for package.</returns>
        public DBPFFile GetPackageByPath(string path)
        {
            path = Filesystem.GetRealPath(path);
            if (_entryByPath.ContainsKey(path))
                return _entryByPath[path];
            else
                return null;
        }

        public void Dispose()
        {
            foreach(var element in _contentEntries)
            {
                element.Dispose();
            }
        }
    }
}
