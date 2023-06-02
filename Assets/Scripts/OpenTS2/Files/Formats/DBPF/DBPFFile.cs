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
using OpenTS2.Content.DBPF;
using UnityEngine;

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
            private readonly DBPFFile _owner;
            private ContentProvider Provider
            {
                get
                {
                    return _owner.Provider;
                }
            }

            public bool Dirty = false;
            public Dictionary<ResourceKey, bool> DeletedEntries = new Dictionary<ResourceKey, bool>();
            public Dictionary<ResourceKey, DynamicDBPFEntry> ChangedEntries = new Dictionary<ResourceKey, DynamicDBPFEntry>();

            public DBPFFileChanges(DBPFFile owner)
            {
                this._owner = owner;
            }

            /// <summary>
            /// Mark all entries in this package as deleted.
            /// todo - be less lazy and make this more efficient.
            /// </summary>
            public void Delete()
            {
                Provider?.RemoveFromResourceMap(_owner);
                var entries = _owner.Entries;
                foreach(var element in entries)
                {
                    DeletedEntries[element.TGI] = true;
                }
                Dirty = true;
                RefreshCache();
            }

            /// <summary>
            /// Revert all changes
            /// </summary>
            public void Clear()
            {
                Provider?.RemoveFromResourceMap(_owner);
                DeletedEntries.Clear();
                ChangedEntries.Clear();
                Dirty = false;
                Provider?.UpdateOrAddToResourceMap(_owner);
                RefreshCache();
            }

            void RefreshCache()
            {
                Provider?.Cache.RemoveAllForPackage(_owner);
            }

            void RefreshCache(ResourceKey tgi)
            {
                Provider?.Cache.Remove(tgi, _owner);
            }

            /// <summary>
            /// Mark an entry as deleted.
            /// </summary>
            /// <param name="entry">Entry to delete</param>
            public void Delete(DBPFEntry entry)
            {
                DeletedEntries[entry.TGI] = true;
                Dirty = true;
                Provider?.RemoveFromResourceMap(entry);
                RefreshCache(entry.TGI);
            }

            /// <summary>
            /// Unmark an entry as deleted.
            /// </summary>
            /// <param name="entry">Entry to undelete</param>
            public void Restore(DBPFEntry entry)
            {
                if (DeletedEntries.ContainsKey(entry.TGI))
                {
                    DeletedEntries.Remove(entry.TGI);
                    Dirty = true;
                    Provider?.UpdateOrAddToResourceMap(entry);
                    RefreshCache(entry.TGI);
                }
            }

            /// <summary>
            /// Mark an entry as deleted by its TGI
            /// </summary>
            /// <param name="tgi">TGI of entry to delete.</param>
            public void Delete(ResourceKey tgi)
            {
                DeletedEntries[tgi] = true;
                Dirty = true;
                Provider?.RemoveFromResourceMap(tgi.LocalGroupID(_owner.GroupID), _owner);
                RefreshCache(tgi);
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
                    Dirty = true;
                    Provider?.UpdateOrAddToResourceMap(_owner.GetEntryByTGI(tgi));
                    RefreshCache(tgi);
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
                asset.Package = _owner;
                if (!ChangedEntries.TryGetValue(asset.TGI, out DynamicDBPFEntry changedEntry))
                    changedEntry = new DynamicDBPFEntry();
                changedEntry.Package = _owner;
                changedEntry.TGI = asset.TGI;
                
                var changedAsset = new ChangedResourceDataAsset(asset, Codecs.Get(asset.TGI.TypeID));
                changedEntry.Change.Data = changedAsset;
                changedEntry.Change.Compressed = asset.Compressed;
                ChangedEntries[asset.TGI] = changedEntry;
                InternalRestore(asset.TGI);
                Dirty = true;
                Provider?.UpdateOrAddToResourceMap(changedEntry);
                RefreshCache(asset.TGI);
            }
            /// <summary>
            /// Save changes to a resource in memory.
            /// </summary>
            /// <param name="bytes">Resource file bytes.</param>
            /// <param name="tgi">Resource TGI.</param>
            /// <param name="compressed">Compress?</param>
            public void Set(byte[] bytes, ResourceKey tgi, bool compressed)
            {
                var changedData = new ChangedResourceDataBytes(bytes, tgi, Codecs.Get(tgi.TypeID), _owner);
                if (!ChangedEntries.TryGetValue(tgi, out DynamicDBPFEntry changedEntry))
                    changedEntry = new DynamicDBPFEntry();
                changedEntry.TGI = tgi;
                changedEntry.Change.Data = changedData;
                changedEntry.Change.Compressed = compressed;
                ChangedEntries[tgi] = changedEntry;
                InternalRestore(tgi);
                Dirty = true;
                Provider?.UpdateOrAddToResourceMap(changedEntry);
                RefreshCache(tgi);
            }

            /// <summary>
            /// Sets compression for a resource.
            /// </summary>
            /// <param name="tgi">Resource TGI.</param>
            /// <param name="compressed">Compress?</param>
            public void SetCompressed(DBPFEntry entry, bool compressed)
            {
                if (!ChangedEntries.TryGetValue(entry.TGI, out DynamicDBPFEntry changedEntry))
                {
                    changedEntry = new DynamicDBPFEntry();
                    changedEntry.Change.Data = new ChangedResourceDataEntry(entry);
                }
                changedEntry.TGI = entry.TGI;
                changedEntry.Change.Compressed = compressed;
                ChangedEntries[entry.TGI] = changedEntry;
                InternalRestore(entry.TGI);
                Dirty = true;
                Provider?.UpdateOrAddToResourceMap(changedEntry);
                RefreshCache(entry.TGI);
            }
        }

        /// <summary>
        /// DIR resource at the time of deserialization.
        /// </summary>
        private DIRAsset _compressionDIR = null;
        public bool Deleted
        {
            get
            {
                return _deleted;
            }
        }
        bool _deleted = false;
        public bool DeleteIfEmpty = true;
        private DBPFFileChanges _changes;
        public ContentProvider Provider = null;
        
        /// <summary>
        /// Holds all runtime modifications in memory.
        /// </summary>
        public DBPFFileChanges Changes
        {
            get
            {
                return _changes;
            }
        }
        private string _filePath = "";
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                var oldProvider = Provider;
                oldProvider?.RemovePackage(this);
                _filePath = value;
                GroupID = FileUtils.GroupHash(Path.GetFileNameWithoutExtension(_filePath));
                oldProvider?.AddPackage(this);
            }
        }
        public int DateCreated;
        public int DateModified;

        public uint IndexMajorVersion;
        public uint IndexMinorVersion;
        private uint _numEntries;
        public uint GroupID;
        private IoBuffer _reader;

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
                    if (Changes.DeletedEntries.ContainsKey(element.TGI))
                        continue;
                    if (!Changes.ChangedEntries.ContainsKey(element.TGI))
                        finalEntries.Add(element);
                }
                foreach(var element in Changes.ChangedEntries)
                {
                    finalEntries.Add(element.Value);
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
                return _entriesList;
            }
        }
        private List<DBPFEntry> _entriesList = new List<DBPFEntry>();
        private Dictionary<ResourceKey, DBPFEntry> _entryByTGI = new Dictionary<ResourceKey, DBPFEntry>();
        //private Dictionary<ResourceKey, DBPFEntry> m_EntryByInternalTGI = new Dictionary<ResourceKey, DBPFEntry>();

        private Stream _stream;
        private IoBuffer _io;

        /// <summary>
        /// Constructs a new DBPF instance.
        /// </summary>
        public DBPFFile()
        {
            _changes = new DBPFFileChanges(this)
            {
                Dirty = true
            };
        }

        /// <summary>
        /// Creates a DBPF instance from a path.
        /// </summary>
        /// <param name="file">The path to an DBPF archive.</param>
        public DBPFFile(string file) : this()
        {
            _filePath = file;
            GroupID = FileUtils.GroupHash(Path.GetFileNameWithoutExtension(file));
            var stream = File.OpenRead(file);
            Read(stream);
            _changes.Dirty = false;
        }

        /// <summary>
        /// Reads a DBPF archive from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public void Read(Stream stream)
        {
            _entryByTGI = new Dictionary<ResourceKey, DBPFEntry>();
            _entriesList = new List<DBPFEntry>();

            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            _reader = io;
            this._io = io;
            this._stream = stream;

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

            _numEntries = io.ReadUInt32();
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
            for (int i = 0; i < _numEntries; i++)
            {
                var entry = new DBPFEntry();
                uint instanceHigh = 0x00000000;
                var TypeID = io.ReadUInt32();
                var EntryGroupID = io.ReadUInt32();
                var InstanceID = io.ReadUInt32();
                if (IndexMinorVersion >= 2)
                    instanceHigh = io.ReadUInt32();
                entry.TGI = new ResourceKey(InstanceID, instanceHigh, EntryGroupID, TypeID);
                entry.FileOffset = io.ReadUInt32();
                entry.FileSize = io.ReadUInt32();
                entry.Package = this;

                _entriesList.Add(entry);
                _entryByTGI[entry.TGI] = entry;
            }
            _compressionDIR = (DIRAsset)GetAssetByTGI(ResourceKey.DIR);
        }

        /// <summary>
        /// Write and clear all changes to FilePath.
        /// </summary>
        public void WriteToFile()
        {
            if (DeleteIfEmpty && Empty)
            {
                Dispose();
                Provider?.RemovePackage(this);
                File.Delete(FilePath);
                Changes.Clear();
                _deleted = true;
                return;
            }
            var data = Serialize();
            Dispose();
            Filesystem.Write(FilePath, data);
            var stream = File.OpenRead(FilePath);
            Read(stream);
            _changes = new DBPFFileChanges(this);
            return;
        }

        /// <summary>
        /// Serializes package with all resource changes, additions and deletions.
        /// </summary>
        /// <returns>Package bytes</returns>
        public byte[] Serialize()
        {
            UpdateDIR();
            var wStream = new MemoryStream(0);
            var writer = new BinaryWriter(wStream);
            var dirAsset = GetAssetByTGI<DIRAsset>(ResourceKey.DIR);
            var entries = Entries;
            //HeeeADER
            writer.Write(new char[] { 'D', 'B', 'P', 'F' });
            //major version
            writer.Write((int)1);
            //minor version
            writer.Write((int)2);

            //unknown
            writer.Write(new byte[12]);

            //Date stuff
            writer.Write((int)0);
            writer.Write((int)0);

            //Index major
            writer.Write((int)7);

            //Num entries
            writer.Write((int)entries.Count);

            //Index offset
            var indexOff = wStream.Position;
            //Placeholder
            writer.Write((int)0);

            //Index size
            var indexSize = wStream.Position;
            //Placeholder
            writer.Write((int)0);

            //Trash Entry Stuff
            writer.Write((int)0);
            writer.Write((int)0);
            writer.Write((int)0);

            //Index Minor Ver
            writer.Write((int)2);
            //Padding
            writer.Write(new byte[32]);

            //Go back and write index offset
            var lastPos = wStream.Position;
            wStream.Position = indexOff;
            writer.Write((int)lastPos);
            wStream.Position = lastPos;

            var entryOffset = new List<long>();

            for(var i=0;i<entries.Count;i++)
            {
                var element = entries[i];
                writer.Write(element.TGI.TypeID);
                writer.Write(element.TGI.GroupID);
                writer.Write(element.TGI.InstanceID);
                writer.Write(element.TGI.InstanceHigh);
                entryOffset.Add(wStream.Position);
                writer.Write(0);
                //File Size
                writer.Write(element.FileSize);
            }

            lastPos = wStream.Position;
            var siz = lastPos - indexOff;
            wStream.Position = indexSize;
            writer.Write((int)siz);
            wStream.Position = lastPos;

            //Write files
            for (var i = 0; i < entries.Count; i++)
            {
                var filePosition = wStream.Position;
                wStream.Position = entryOffset[i];
                writer.Write((int)filePosition);
                wStream.Position = filePosition;
                var entry = entries[i];
                if (!(entry is DynamicDBPFEntry))
                {
                    var rawData = GetRawBytes(entry);
                    writer.Write(rawData, 0, rawData.Length);
                }
                else
                {
                    var entryData = entry.GetBytes();
                    if (dirAsset != null && dirAsset.GetUncompressedSize(entry.TGI) != 0)
                    {
                        entryData = DBPFCompression.Compress(entryData);
                        var lastPosition = wStream.Position;
                        wStream.Position = entryOffset[i] + 4;
                        writer.Write(entryData.Length);
                        wStream.Position = lastPosition;
                    }
                    writer.Write(entryData, 0, entryData.Length);
                }
            }
            
            var buffer = StreamUtils.GetBuffer(wStream);
            writer.Dispose();
            wStream.Dispose();
            return buffer;
        }

        void UpdateDIR()
        {
            var dirAsset = new DIRAsset();
            var entries = Entries;
            foreach(var element in entries)
            {
                if (element is DynamicDBPFEntry dynamicEntry)
                {
                    if (dynamicEntry.Change.Compressed)
                        dirAsset.SizeByInternalTGI[element.TGI] = (uint)dynamicEntry.FileSize;
                }
                else
                {
                    var uncompressedSize = InternalGetUncompressedSize(element);
                    if (uncompressedSize > 0)
                        dirAsset.SizeByInternalTGI[element.TGI] = uncompressedSize;
                }
            }
            if (dirAsset.SizeByInternalTGI.Count == 0)
            {
                Changes.Delete(ResourceKey.DIR);
                return;
            }
            dirAsset.Package = this;
            dirAsset.TGI = ResourceKey.DIR;
            dirAsset.Compressed = false;
            dirAsset.Save();
        }

        /// <summary>
        /// Gets a DBPFEntry's data from this DBPF instance.
        /// </summary>
        /// <param name="entry">Entry to retrieve data for.</param>
        /// <returns>Data for entry.</returns>
        public byte[] GetBytes(DBPFEntry entry, bool ignoreChanges = false)
        {
            if (!ignoreChanges)
            {
                if (Changes.DeletedEntries.ContainsKey(entry.TGI))
                    return null;
                if (Changes.ChangedEntries.ContainsKey(entry.TGI))
                    return Changes.ChangedEntries[entry.TGI].Change.Data.GetBytes();
            }
            _reader.Seek(SeekOrigin.Begin, entry.FileOffset);
            var fileBytes = _reader.ReadBytes((int)entry.FileSize);
            var uncompressedSize = InternalGetUncompressedSize(entry);
            if (uncompressedSize > 0)
            {
                return DBPFCompression.Decompress(fileBytes, uncompressedSize);
            }
            return fileBytes;
        }

        private byte[] GetRawBytes(DBPFEntry entry)
        {
            _reader.Seek(SeekOrigin.Begin, entry.FileOffset);
            var fileBytes = _reader.ReadBytes((int)entry.FileSize);
            return fileBytes;
        }

        /// <summary>
        /// Gets an item from its TGI (Type, Group, Instance IDs)
        /// </summary>
        /// <param name="tgi">The TGI of the entry.</param>
        /// <returns>The entry's data.</returns>
        public byte[] GetBytesByTGI(ResourceKey tgi, bool ignoreChanges = false)
        {
            if (!ignoreChanges)
            {
                if (Changes.DeletedEntries.ContainsKey(tgi))
                    return null;
                if (Changes.ChangedEntries.ContainsKey(tgi))
                    return Changes.ChangedEntries[tgi].Change.Data.GetBytes();
            }
            if (_entryByTGI.ContainsKey(tgi))
                return GetBytes(_entryByTGI[tgi]);
            else
                return null;
        }

        uint InternalGetUncompressedSize(DBPFEntry entry)
        {
            if (entry.TGI.TypeID == TypeIDs.DIR)
                return 0;
            var dirAsset = _compressionDIR;
            if (dirAsset == null)
                return 0;
            if (dirAsset.SizeByInternalTGI.ContainsKey(entry.TGI))
                return dirAsset.SizeByInternalTGI[entry.TGI];
            return 0;
        }

        /// <summary>
        /// Gets an asset from its DBPF Entry
        /// </summary>
        /// <param name="entry">The DBPF Entry</param>
        /// <returns></returns>
        public AbstractAsset GetAsset<T>(DBPFEntry entry, bool ignoreChanges = false) where T : AbstractAsset
        {
            return GetAsset(entry, ignoreChanges) as T;
        }

        /// <summary>
        /// Gets an asset from its DBPF Entry
        /// </summary>
        /// <param name="entry">The DBPF Entry</param>
        /// <returns></returns>
        public AbstractAsset GetAsset(DBPFEntry entry, bool ignoreChanges = false)
        {
            if (Changes.DeletedEntries.ContainsKey(entry.TGI) && !ignoreChanges)
                return null;
            if (Changes.ChangedEntries.ContainsKey(entry.TGI) && !ignoreChanges)
                return Changes.ChangedEntries[entry.TGI].Change.Data.GetAsset();
            var item = GetBytes(entry, ignoreChanges);
            var codec = Codecs.Get(entry.GlobalTGI.TypeID);
            if (codec == null)
            {
                throw new ArgumentException($"No codec to handle type {entry.GlobalTGI.TypeID}");
            }
            var asset = codec.Deserialize(item, entry.GlobalTGI, this);
            asset.Compressed = InternalGetUncompressedSize(entry) > 0;
            asset.TGI = entry.TGI;
            asset.Package = this;
            return asset;
        }

        public bool IsCompressed(DBPFEntry entry)
        {
            return InternalGetUncompressedSize(entry) > 0;
        }

        public T GetAssetByTGI<T>(ResourceKey tgi, bool ignoreChanges = false) where T : AbstractAsset
        {
            return GetAssetByTGI(tgi, ignoreChanges) as T;
        }
        public AbstractAsset GetAssetByTGI(ResourceKey tgi, bool ignoreChanges = false)
        {
            var entry = GetEntryByTGI(tgi, ignoreChanges);
            if (entry != null)
                return GetAsset(entry, ignoreChanges);
            else
                return null;
        }

        /// <summary>
        /// Gets an entry from its TGI (Type, Group, Instance IDs)
        /// </summary>
        /// <param name="tgi">The TGI of the entry.</param>
        /// <returns>The entry.</returns>
        public DBPFEntry GetEntryByTGI(ResourceKey tgi , bool ignoreChanges = false)
        {
            if (Changes.DeletedEntries.ContainsKey(tgi) && !ignoreChanges)
                return null;
            if (Changes.ChangedEntries.ContainsKey(tgi) && !ignoreChanges)
                return Changes.ChangedEntries[tgi];
            if (_entryByTGI.ContainsKey(tgi))
                return _entryByTGI[tgi];
            else
                return null;
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes this DBPF instance.
        /// </summary>
        public void Dispose()
        {
            _stream?.Dispose();
            _io?.Dispose();
        }

        #endregion
    }
}