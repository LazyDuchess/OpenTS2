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

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// The database-packed file (DBPF) is a format used to store data for pretty much all Maxis games after The Sims, 
    /// including The Sims Online (the first appearance of this format), SimCity 4, The Sims 2, Spore, The Sims 3, and 
    /// SimCity 2013.
    /// </summary>
    public class DBPFFile : IDisposable
    {
        public int DateCreated;
        public int DateModified;

        private uint IndexMajorVersion;
        private uint IndexMinorVersion;
        private uint NumEntries;
        public uint GroupID;
        private IoBuffer m_Reader;

        private List<DBPFEntry> m_EntriesList = new List<DBPFEntry>();
        private Dictionary<TGI, DBPFEntry> m_EntryByTGI = new Dictionary<TGI, DBPFEntry>();
        private Dictionary<uint, List<DBPFEntry>> m_EntriesByType = new Dictionary<uint, List<DBPFEntry>>();

        private IoBuffer Io;

        /// <summary>
        /// Constructs a new DBPF instance.
        /// </summary>
        public DBPFFile()
        {
        }

        /// <summary>
        /// Creates a DBPF instance from a path.
        /// </summary>
        /// <param name="file">The path to an DBPF archive.</param>
        public DBPFFile(string file)
        {
            GroupID = FileUtils.GroupHash(Path.GetFileNameWithoutExtension(ContentManager.FileSystem.GetRealPath(file)));
            var stream = ContentManager.FileSystem.OpenRead(file);
            Read(stream);
        }

        /// <summary>
        /// Reads a DBPF archive from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public void Read(Stream stream)
        {
            m_EntryByTGI = new Dictionary<TGI, DBPFEntry>();
            m_EntriesList = new List<DBPFEntry>();

            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            m_Reader = io;
            this.Io = io;

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
                if (EntryGroupID == 0xFFFFFFFF)
                    EntryGroupID = GroupID;
                var InstanceID = io.ReadUInt32();
                if (IndexMinorVersion >= 2)
                    instanceHigh = io.ReadUInt32();
                entry.tgi = new TGI(InstanceID, instanceHigh, EntryGroupID, TypeID);
                entry.FileOffset = io.ReadUInt32();
                entry.FileSize = io.ReadUInt32();

                m_EntriesList.Add(entry);
                //ulong id = (((ulong)entry.InstanceID) << 32) + (ulong)entry.TypeID;
                if (!m_EntryByTGI.ContainsKey(entry.tgi))
                    m_EntryByTGI.Add(entry.tgi, entry);

                if (!m_EntriesByType.ContainsKey(entry.tgi.TypeID))
                    m_EntriesByType.Add(entry.tgi.TypeID, new List<DBPFEntry>());

                m_EntriesByType[entry.tgi.TypeID].Add(entry);
            }
        }

        /// <summary>
        /// Gets a DBPFEntry's data from this DBPF instance.
        /// </summary>
        /// <param name="entry">Entry to retrieve data for.</param>
        /// <returns>Data for entry.</returns>
        public byte[] GetEntry(DBPFEntry entry)
        {
            m_Reader.Seek(SeekOrigin.Begin, entry.FileOffset);
            return m_Reader.ReadBytes((int)entry.FileSize);
        }

        /// <summary>
        /// Gets an entry from its TGI (Type, Group, Instance IDs)
        /// </summary>
        /// <param name="tgi">The TGI of the entry.</param>
        /// <returns>The entry's data.</returns>
        public byte[] GetItemByTGI(TGI tgi)
        {
            if (m_EntryByTGI.ContainsKey(tgi))
                return GetEntry(m_EntryByTGI[tgi]);
            else
                return null;
        }

        /// <summary>
        /// Gets all entries of a specific type.
        /// </summary>
        /// <param name="Type">The Type of the entry.</param>
        /// <returns>The entry data, paired with its TGI.</returns>
        public List<KeyValuePair<TGI, byte[]>> GetItemsByType(uint Type)
        {

            var result = new List<KeyValuePair<TGI, byte[]>>();

            var entries = m_EntriesByType[Type];
            for (int i = 0; i < entries.Count; i++)
            {
                result.Add(new KeyValuePair<TGI, byte[]>(entries[i].tgi, GetEntry(entries[i])));
            }
            return result;
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes this DBPF instance.
        /// </summary>
        public void Dispose()
        {
            Io.Dispose();
        }

        #endregion
    }
}