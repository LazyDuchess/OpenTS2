/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;

namespace OpenTS2.Content
{
    /// <summary>
    /// Provides access to reading and writing to assets in packages.
    /// </summary>
    public class ContentProvider : IDisposable
    {
        public List<DBPFFile> ContentEntries
        {
            get { return _contentEntries; }
        }

        private List<DBPFFile> _contentEntries = new List<DBPFFile>();
        private Dictionary<uint, DBPFFile> entryByGroupID = new Dictionary<uint, DBPFFile>();
        private Dictionary<string, DBPFFile> entryByPath = new Dictionary<string, DBPFFile>();
        private Dictionary<DBPFFile, DBPFFile> entryByFile = new Dictionary<DBPFFile, DBPFFile>();
        public ContentCache Cache = new ContentCache();
        public ContentChanges Changes;
        private Files.Filesystem fileSystem;

        public ContentProvider(Files.Filesystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.Changes = new ContentChanges(this, fileSystem);
        }

        AbstractAsset InternalLoadAsset(CacheKey key)
        {
            if (key.file == null)
            {
                foreach (var element in _contentEntries)
                {
                    var localTGI = key.tgi.GlobalGroupID(element.GroupID);
                    var entryByTGI = element.GetEntryByTGI(localTGI);
                    if (entryByTGI != null)
                    {
                        var finalAsset = element.GetAsset(entryByTGI);
                        return finalAsset;
                    }
                }
            }
            else
            {
                return key.file.GetAssetByTGI(key.tgi);
            }
            return null;
        }

        /// <summary>
        /// Caches an asset into the content system and returns it.
        /// </summary>
        /// <param name="tgi">TGI of the resource.</param>
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

        /// <summary>
        /// Adds a DBPF Package to the filesystem, or returns existing package if already loaded.
        /// </summary>
        /// <param name="path">Path to the package.</param>
        /// <returns>Content entry for package.</returns>
        public DBPFFile AddPackage(string path)
        {
            var realPath = fileSystem.GetRealPath(path);
            if (entryByPath.ContainsKey(realPath))
                return entryByPath[realPath];
            var package = new DBPFFile(path);
            AddPackage(package);
            return package;
        }

        /// <summary>
        /// Adds a DBPF Package to the filesystem. Removes any packages with the same path.
        /// </summary>
        /// <param name="package">DBPFFile to add.</param>
        /// <returns>Content entry for package.</returns>
        public void AddPackage(DBPFFile package)
        {
            if (entryByPath.ContainsKey(package.FilePath))
            {
                RemovePackage(entryByPath[package.FilePath]);
            }
            _contentEntries.Insert(0, package);
            entryByGroupID[package.GroupID] = package;
            entryByPath[package.FilePath] = package;
            entryByFile[package] = package;
            package.OnRenameEvent += PackageOnRename;
        }

        void PackageOnRename(DBPFFile package, string oldName, uint oldGroupID)
        {
            Cache.RemoveAllForPackage(package);
            var entry = entryByFile[package];
            entryByPath.Remove(oldName);
            entryByGroupID.Remove(oldGroupID);
            entryByPath[package.FilePath] = entry;
            entryByGroupID[package.GroupID] = entry;
        }

        /// <summary>
        /// Removes a DBPF Package from the filesystem.
        /// </summary>
        /// <param name="package">Content entry for the package to remove.</param>
        public void RemovePackage(DBPFFile package)
        {
            //package.Dispose();
            Cache.RemoveAllForPackage(package);
            _contentEntries.Remove(package);
            entryByGroupID.Remove(package.GroupID);
            entryByPath.Remove(package.FilePath);
            entryByFile.Remove(package);
            package.OnRenameEvent -= PackageOnRename;
        }

        /// <summary>
        /// Gets a DBPF Package by its generated Group ID.
        /// </summary>
        /// <param name="groupID">Group ID.</param>
        /// <returns>Content entry for package.</returns>
        public DBPFFile GetPackageByGroup(uint groupID)
        {
            if (entryByGroupID.ContainsKey(groupID))
                return entryByGroupID[groupID];
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
            if (entryByGroupID.ContainsKey(groupID))
                return entryByGroupID[groupID];
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
            path = fileSystem.GetRealPath(path);
            if (entryByPath.ContainsKey(path))
                return entryByPath[path];
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
