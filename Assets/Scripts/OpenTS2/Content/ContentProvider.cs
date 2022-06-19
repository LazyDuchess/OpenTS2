/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Common.Utils;
using OpenTS2.Common;

namespace OpenTS2.Content
{
    public class ContentEntry
    {
        public DBPFFile file;
        public string path;
        public uint GroupID
        {
            get { return file.GroupID; }
        }
    }

    /// <summary>
    /// Provides access to reading and writing to assets in packages.
    /// </summary>
    public class ContentProvider : IDisposable
    {
        public List<ContentEntry> ContentEntries
        {
            get { return _contentEntries; }
        }

        private List<ContentEntry> _contentEntries = new List<ContentEntry>();
        private Dictionary<uint, ContentEntry> entryByGroupID = new Dictionary<uint, ContentEntry>();
        private Dictionary<string, ContentEntry> entryByPath = new Dictionary<string, ContentEntry>();
        private ContentCache contentCache = new ContentCache();

        AbstractAsset InternalLoadAsset(ResourceKey tgi)
        {
            foreach (var element in _contentEntries)
            {
                var itemByTGI = element.file.GetItemByTGI(tgi);
                if (itemByTGI != null)
                {
                    var codec = Codecs.GetCodecInstanceForType(tgi.TypeID);
                    var finalAsset = codec.Deserialize(itemByTGI, tgi, element.path);
                    finalAsset.tgi = tgi;
                    finalAsset.sourceFile = element.path;
                    return finalAsset;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Caches an asset into the content system and returns it.
        /// </summary>
        /// <param name="tgi">TGI of the resource.</param>
        /// <returns>The asset.</returns>
        public AbstractAsset GetAsset(ResourceKey tgi)
        {
            return contentCache.GetOrAdd(tgi, InternalLoadAsset);
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
        /// Adds a DBPF Package to the filesystem.
        /// </summary>
        /// <param name="path">Path to the package.</param>
        /// <returns>Content entry for package.</returns>
        public ContentEntry AddPackage(string path)
        {
            var package = new DBPFFile(path);
            var realPath = ContentManager.FileSystem.GetRealPath(path);
            var contentEntry = new ContentEntry()
            {
                file = package,
                path = realPath
            };
            _contentEntries.Insert(0, contentEntry);
            entryByGroupID[contentEntry.GroupID] = contentEntry;
            entryByPath[realPath] = contentEntry;
            return contentEntry;
        }

        /// <summary>
        /// Removes a DBPF Package from the filesystem.
        /// </summary>
        /// <param name="package">Content entry for the package to remove.</param>
        public void RemovePackage(ContentEntry package)
        {
            package.file.Dispose();
            _contentEntries.Remove(package);
            entryByGroupID.Remove(package.GroupID);
            entryByPath.Remove(package.path);
        }

        /// <summary>
        /// Gets a DBPF Package by its generated Group ID.
        /// </summary>
        /// <param name="groupID">Group ID.</param>
        /// <returns>Content entry for package.</returns>
        public ContentEntry GetPackageByGroup(uint groupID)
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
        public ContentEntry GetPackageByGroup(string groupName)
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
        public ContentEntry GetPackageByPath(string path)
        {
            path = ContentManager.FileSystem.GetRealPath(path);
            if (entryByPath.ContainsKey(path))
                return entryByPath[path];
            else
                return null;
        }

        public void Dispose()
        {
            foreach(var element in _contentEntries)
            {
                element.file.Dispose();
            }
        }
    }
}
