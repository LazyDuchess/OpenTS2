/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content;
using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.ARC
{
    /// <summary>
    /// Archive files used to store data in the PlayStation 2 version of The Sims 2.
    /// </summary>
    public class ARCFile : IDisposable
    {
        public List<ARCEntry> Entries
        {
            get { return _Entries; }
        }
        private List<ARCEntry> _Entries = new List<ARCEntry>();
        private Dictionary<string, ARCEntry> EntryByName = new Dictionary<string, ARCEntry>();
        private IoBuffer Io;

        /// <summary>
        /// Constructs a new ARC instance.
        /// </summary>
        public ARCFile()
        {
        }

        /// <summary>
        /// Creates an ARC instance from a path.
        /// </summary>
        /// <param name="file">The path to an ARC archive.</param>
        public ARCFile(string file)
        {
            var stream = ContentManager.FileSystem.OpenRead(file);
            Read(stream);
        }

        /// <summary>
        /// Reads an ARC archive from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public void Read(Stream stream)
        {
            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            Io = io;
            var fileOffset = io.ReadUInt32();
            io.Seek(SeekOrigin.Begin, fileOffset);
            var fileAmount = io.ReadUInt32();
            for (var i=0;i<fileAmount;i++)
            {
                var entry = new ARCEntry();
                io.Skip(4);
                var offset = io.ReadUInt32();
                var size = io.ReadUInt32();
                var filename = io.ReadNullTerminatedString();
                io.Skip(8);
                entry.FileName = filename;
                entry.FileOffset = offset;
                entry.FileSize = size;
                _Entries.Add(entry);
                EntryByName[entry.FileName] = entry;
            }
        }

        public byte[] GetEntryNoHeader(ARCEntry entry)
        {
            Io.Seek(SeekOrigin.Begin, entry.FileOffset);
            var oldPosition = (int)Io.Position;
            Io.Skip(16);
            Io.ReadNullTerminatedString();
            Io.Skip(4);
            var offset = (int)Io.Position - oldPosition;
            return Io.ReadBytes((int)entry.FileSize - offset);
        }

        public byte[] GetEntry(ARCEntry entry)
        {
            Io.Seek(SeekOrigin.Begin, entry.FileOffset);

            return Io.ReadBytes((int)entry.FileSize);
        }

        public byte[] GetItemByName(string filename)
        {
            if (EntryByName.ContainsKey(filename))
                return GetEntryNoHeader(EntryByName[filename]);
            else
                return null;
        }

        public void Dispose()
        {
            Io.Dispose();
        }
    }
}
