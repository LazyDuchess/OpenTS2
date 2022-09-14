/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTS2.Files.Utils;
using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.Changes;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// The database-packed file (DBPF) is a format used to store data for pretty much all Maxis games after The Sims, 
    /// including The Sims Online (the first appearance of this format), SimCity 4, The Sims 2, Spore, The Sims 3, and 
    /// SimCity 2013.
    /// </summary>
    public class DBPFFile : IDisposable
    {
        public class DBPFFileChanges
        {
            private DBPFFile owner;
            public bool Dirty = false;
            public Dictionary<ResourceKey, bool> DeletedEntries = new Dictionary<ResourceKey, bool>();
            public Dictionary<ResourceKey, AbstractChanged> ChangedEntries = new Dictionary<ResourceKey, AbstractChanged>();

            public DBPFFileChanges(DBPFFile owner)
            {
                this.owner = owner;
            }

            /// <summary>
            /// Mark all entries in this package as deleted.
            /// </summary>
            public void DeleteAll()
            {
                foreach(var element in owner.m_EntriesList)
                {
                    DeletedEntries[element.internalTGI] = true;
                    RefreshCache(element.internalTGI);
                }
                Dirty = true;
            }

            /// <summary>
            /// Revert all changes
            /// </summary>
            public void Clear()
            {
                DeletedEntries.Clear();
                ChangedEntries.Clear();
                Dirty = true;
            }

            void RefreshCache(ResourceKey tgi)
            {
                tgi = tgi.LocalGroupID(owner.GroupID);

                var reference = ContentManager.Cache.GetWeakReference(tgi);
                if (reference != null && reference.IsAlive && reference.Target != null && reference.Target is AbstractAsset asset)
                {
                    if (asset.package == owner)
                        ContentManager.Cache.Remove(tgi);
                }

                reference = ContentManager.Cache.GetWeakReference(tgi, owner);
                if (reference != null && reference.IsAlive && reference.Target != null && reference.Target is AbstractAsset asset2)
                {
                    ContentManager.Cache.Remove(tgi, owner);
                }
            }

            /// <summary>
            /// Mark an entry as deleted.
            /// </summary>
            /// <param name="entry">Entry to delete</param>
            public void Delete(DBPFEntry entry)
            {
                DeletedEntries[entry.internalTGI] = true;
                RefreshCache(entry.internalTGI);
                Dirty = true;
            }

            /// <summary>
            /// Unmark an entry as deleted.
            /// </summary>
            /// <param name="entry">Entry to undelete</param>
            public void Restore(DBPFEntry entry)
            {
                if (DeletedEntries.ContainsKey(entry.internalTGI))
                {
                    DeletedEntries.Remove(entry.internalTGI);
                    RefreshCache(entry.internalTGI);
                    Dirty = true;
                }
            }

            /// <summary>
            /// Mark an entry as deleted by its TGI
            /// </summary>
            /// <param name="tgi">TGI of entry to delete.</param>
            public void Delete(ResourceKey tgi)
            {
                DeletedEntries[tgi] = true;
                RefreshCache(tgi);
                Dirty = true;
            }
            /// <summary>
            /// Unmark an entry as deleted by its TGI
            /// </summary>
            /// <param name="tgi">TGI of entry to undelete.</param>
            public void Restore(ResourceKey tgi)
            {
                if (DeletedEntries.ContainsKey(tgi))
                {
                    DeletedEntries.Remove(tgi);
                    RefreshCache(tgi);
                    Dirty = true;
                }
            }

            /// <summary>
            /// Unmark an entry as deleted without updating cache.
            /// </summary>
            /// <param name="tgi">TGI of entry to undelete.</param>
            void InternalRestore(ResourceKey tgi)
            {
                if (DeletedEntries.ContainsKey(tgi))
                {
                    DeletedEntries.Remove(tgi);
                }
            }

            /// <summary>
            /// Save changes to an asset in memory.
            /// </summary>
            /// <param name="asset">Asset.</param>
            public void Set(AbstractAsset asset)
            {
                asset.package = owner;
                var changedAsset = new ChangedAsset(asset);
                ChangedEntries[asset.internalTGI] = changedAsset;
                InternalRestore(asset.internalTGI);
                RefreshCache(asset.internalTGI);
                Dirty = true;
            }
            /// <summary>
            /// Save changes to a resource in memory.
            /// </summary>
            /// <param name="bytes">Resource file bytes.</param>
            /// <param name="tgi">Resource TGI.</param>
            /// <param name="compressed">Compress?</param>
            public void Set(byte[] bytes, ResourceKey tgi, bool compressed)
            {
                var changedFile = new ChangedFile(bytes, tgi, owner, Codecs.Get(tgi.TypeID));
                changedFile.compressed = compressed;
                ChangedEntries[tgi] = changedFile;
                InternalRestore(tgi);
                RefreshCache(tgi);
                Dirty = true;
            }
        }

        bool DeleteIfEmpty = true;
        private DBPFFileChanges m_changes;
        /// <summary>
        /// Holds all runtime modifications in memory.
        /// </summary>
        public DBPFFileChanges Changes
        {
            get
            {
                return m_changes;
            }
        }
        private string m_filePath = "";
        public string FilePath
        {
            get { return m_filePath; }
            set
            {
                value = ContentManager.FileSystem.GetRealPath(value);
                var oldName = m_filePath;
                m_filePath = value;
                UpdateFilename(oldName);
            }
        }
        public int DateCreated;
        public int DateModified;

        private uint IndexMajorVersion;
        private uint IndexMinorVersion;
        private uint NumEntries;
        public uint GroupID;
        private IoBuffer m_Reader;

        /// <summary>
        /// Returns true if this package is empty.
        /// </summary>
        public bool Empty
        {
            get
            {
                return Entries.Count == 0;
            }
        }

        /// <summary>
        /// Get all entries in this package, plus modifications, minus deleted entries.
        /// </summary>
        public List<DBPFEntry> Entries
        {
            get
            {
                var basicEntries = OriginalEntries;
                var finalEntries = new List<DBPFEntry>();
                foreach(var element in basicEntries)
                {
                    if (Changes.DeletedEntries.ContainsKey(element.internalTGI))
                        continue;
                    if (Changes.ChangedEntries.ContainsKey(element.internalTGI))
                        finalEntries.Add(Changes.ChangedEntries[element.internalTGI].entry);
                }
                return finalEntries;
            }
        }

        /// <summary>
        /// Get all original entries in this package.
        /// </summary>
        public List<DBPFEntry> OriginalEntries
        {
            get
            {
                return m_EntriesList;
            }
        }
        private List<DBPFEntry> m_EntriesList = new List<DBPFEntry>();
        private Dictionary<ResourceKey, DBPFEntry> m_EntryByTGI = new Dictionary<ResourceKey, DBPFEntry>();
        //private Dictionary<ResourceKey, DBPFEntry> m_EntryByInternalTGI = new Dictionary<ResourceKey, DBPFEntry>();
        private Dictionary<uint, List<DBPFEntry>> m_EntriesByType = new Dictionary<uint, List<DBPFEntry>>();

        private Stream stream;
        private IoBuffer Io;

        public delegate void DBPFFileEvent(DBPFFile file, string oldName, uint oldGroupID);
        public DBPFFileEvent OnRenameEvent;

        void UpdateFilename(string oldName)
        {
            var oldGroupID = GroupID;
            GroupID = FileUtils.GroupHash(Path.GetFileNameWithoutExtension(ContentManager.FileSystem.GetRealPath(m_filePath)));
            OnRenameEvent?.Invoke(this, oldName, oldGroupID);
        }
        /// <summary>
        /// Constructs a new DBPF instance.
        /// </summary>
        public DBPFFile()
        {
            m_changes = new DBPFFileChanges(this);
            m_changes.Dirty = true;
        }

        /// <summary>
        /// Creates a DBPF instance from a path.
        /// </summary>
        /// <param name="file">The path to an DBPF archive.</param>
        public DBPFFile(string file) : this()
        {
            m_filePath = ContentManager.FileSystem.GetRealPath(file);
            GroupID = FileUtils.GroupHash(Path.GetFileNameWithoutExtension(ContentManager.FileSystem.GetRealPath(file)));
            var stream = ContentManager.FileSystem.OpenRead(file);
            Read(stream);
            m_changes.Dirty = false;
        }

        /// <summary>
        /// Reads a DBPF archive from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public void Read(Stream stream)
        {
            m_EntryByTGI = new Dictionary<ResourceKey, DBPFEntry>();
            m_EntriesList = new List<DBPFEntry>();
            m_EntriesByType = new Dictionary<uint, List<DBPFEntry>>();
            //m_EntryByInternalTGI = new Dictionary<ResourceKey, DBPFEntry>();

            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            m_Reader = io;
            this.Io = io;
            this.stream = stream;

            var magic = io.ReadCString(4);
            if (magic != "DBPF")
            {
                throw new Exception("Not a DBPF file");
            }

            var majorVersion = io.ReadUInt32();
            var minorVersion = io.ReadUInt32();
            var version = majorVersion + (((double)minorVersion) / 10.0);

            /** Unknown, set to 0 **/
            io.Skip(12);

            // Changed from FreeSO's "version == 1.0"
            if (version <= 1.2)
            {
                this.DateCreated = io.ReadInt32();
                this.DateModified = io.ReadInt32();
            }

            if (version < 2.0)
            {
                IndexMajorVersion = io.ReadUInt32();
            }

            NumEntries = io.ReadUInt32();
            uint indexOffset = 0;
            if (version < 2.0)
            {
                indexOffset = io.ReadUInt32();
            }
            var indexSize = io.ReadUInt32();
            if (version < 2.0)
            {
                var trashEntryCount = io.ReadUInt32();
                var trashIndexOffset = io.ReadUInt32();
                var trashIndexSize = io.ReadUInt32();
                IndexMinorVersion = io.ReadUInt32();
            }
            else if (version == 2.0)
            {
                IndexMinorVersion = io.ReadUInt32();
                indexOffset = io.ReadUInt32();
                io.Skip(4);
            }

            /** Padding **/
            io.Skip(32);

            io.Seek(SeekOrigin.Begin, indexOffset);
            for (int i = 0; i < NumEntries; i++)
            {
                var entry = new DBPFEntry();
                uint instanceHigh = 0x00000000;
                //entry.tgi = new TGI()
                var TypeID = io.ReadUInt32();
                var EntryGroupID = io.ReadUInt32();
                var InternalGroupID = EntryGroupID;
                if (EntryGroupID == Groups.Local)
                    EntryGroupID = GroupID;
                var InstanceID = io.ReadUInt32();
                if (IndexMinorVersion >= 2)
                    instanceHigh = io.ReadUInt32();
                entry.tgi = new ResourceKey(InstanceID, instanceHigh, EntryGroupID, TypeID);
                entry.internalTGI = new ResourceKey(InstanceID, instanceHigh, InternalGroupID, TypeID);
                entry.FileOffset = io.ReadUInt32();
                entry.FileSize = io.ReadUInt32();

                m_EntriesList.Add(entry);
                //ulong id = (((ulong)entry.InstanceID) << 32) + (ulong)entry.TypeID;
                /*
                if (!m_EntryByTGI.ContainsKey(entry.tgi))
                    m_EntryByTGI.Add(entry.tgi, entry);*/
                if (!m_EntryByTGI.ContainsKey(entry.internalTGI))
                    m_EntryByTGI.Add(entry.internalTGI, entry);
                /*
                if (!m_EntryByInternalTGI.ContainsKey(entry.internalTGI))
                    m_EntryByInternalTGI.Add(entry.internalTGI, entry);*/

                if (!m_EntriesByType.ContainsKey(entry.tgi.TypeID))
                    m_EntriesByType.Add(entry.tgi.TypeID, new List<DBPFEntry>());

                m_EntriesByType[entry.tgi.TypeID].Add(entry);
            }
        }

        public void WriteToFile()
        {
            Dispose();
            if (DeleteIfEmpty && Empty)
            {
                ContentManager.Provider.RemovePackage(this);
                ContentManager.FileSystem.Delete(FilePath);
                Changes.Clear();
                return;
            }
        }

        /// <summary>
        /// Gets a DBPFEntry's data from this DBPF instance.
        /// </summary>
        /// <param name="entry">Entry to retrieve data for.</param>
        /// <returns>Data for entry.</returns>
        public byte[] GetEntry(DBPFEntry entry, bool ignoreDeleted = true)
        {
            if (ignoreDeleted)
            {
                if (Changes.DeletedEntries.ContainsKey(entry.internalTGI))
                    return null;
            }
            if (Changes.ChangedEntries.ContainsKey(entry.internalTGI))
                return Changes.ChangedEntries[entry.internalTGI].bytes;
            m_Reader.Seek(SeekOrigin.Begin, entry.FileOffset);
            return m_Reader.ReadBytes((int)entry.FileSize);
        }

        /// <summary>
        /// Gets an item from its TGI (Type, Group, Instance IDs)
        /// </summary>
        /// <param name="tgi">The TGI of the entry.</param>
        /// <returns>The entry's data.</returns>
        public byte[] GetItemByTGI(ResourceKey tgi, bool ignoreDeleted = true)
        {
            if (ignoreDeleted)
            {
                if (Changes.DeletedEntries.ContainsKey(tgi))
                    return null;
            }
            if (Changes.ChangedEntries.ContainsKey(tgi))
                return Changes.ChangedEntries[tgi].bytes;
            if (m_EntryByTGI.ContainsKey(tgi))
                return GetEntry(m_EntryByTGI[tgi]);
            else
                return null;
        }
        /// <summary>
        /// Gets an asset from its DBPF Entry
        /// </summary>
        /// <param name="entry">The DBPF Entry</param>
        /// <returns></returns>
        public AbstractAsset GetAsset(DBPFEntry entry, bool ignoreDeleted = true)
        {
            if (Changes.ChangedEntries.ContainsKey(entry.internalTGI))
                return Changes.ChangedEntries[entry.internalTGI].asset;
            var item = GetEntry(entry, ignoreDeleted);
            var codec = Codecs.Get(entry.tgi.TypeID);
            var asset = codec.Deserialize(item, entry.tgi, this);
            asset.Compressed = false;
            asset.tgi = entry.tgi;
            asset.internalTGI = entry.internalTGI;
            asset.package = this;
            return asset;
        }

        public AbstractAsset GetAssetByTGI(ResourceKey tgi, bool ignoreDeleted = true)
        {
            return GetAsset(GetEntryByTGI(tgi), ignoreDeleted);
        }

        /// <summary>
        /// Gets an entry from its TGI (Type, Group, Instance IDs)
        /// </summary>
        /// <param name="tgi">The TGI of the entry.</param>
        /// <returns>The entry.</returns>
        public DBPFEntry GetEntryByTGI(ResourceKey tgi , bool ignoreDeleted = true)
        {
            if (ignoreDeleted)
            {
                if (Changes.DeletedEntries.ContainsKey(tgi))
                    return null;
            }
            if (Changes.ChangedEntries.ContainsKey(tgi))
                return Changes.ChangedEntries[tgi].entry;
            if (m_EntryByTGI.ContainsKey(tgi))
                return m_EntryByTGI[tgi];
            else
                return null;
        }

        /// <summary>
        /// Gets all entries of a specific type.
        /// </summary>
        /// <param name="Type">The Type of the entry.</param>
        /// <returns>The entry data, paired with its TGI.</returns>
        public List<KeyValuePair<ResourceKey, byte[]>> GetItemsByType(uint Type, bool ignoreDeleted = true)
        {

            var result = new List<KeyValuePair<ResourceKey, byte[]>>();

            var entries = m_EntriesByType[Type];
            for (int i = 0; i < entries.Count; i++)
            {
                if (ignoreDeleted && Changes.DeletedEntries.ContainsKey(entries[i].internalTGI))
                    continue;
                result.Add(new KeyValuePair<ResourceKey, byte[]>(entries[i].tgi, GetEntry(entries[i])));
            }
            return result;
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes this DBPF instance.
        /// </summary>
        public void Dispose()
        {
            stream?.Dispose();
            Io?.Dispose();
        }

        #endregion
    }
}