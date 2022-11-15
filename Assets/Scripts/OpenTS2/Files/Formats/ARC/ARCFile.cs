/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenTS2.Files.Formats.ARC
{
    /// <summary>
    /// Archive files used to store data in the PlayStation 2 version of The Sims 2.
    /// </summary>
    public class ARCFile : IDisposable
    {
        public List<ARCEntry> Entries
        {
            get { return _entries; }
        }
        private readonly List<ARCEntry> _entries = new List<ARCEntry>();
        private readonly Dictionary<string, ARCEntry> _entryByName = new Dictionary<string, ARCEntry>();
        private IoBuffer _io;

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
            var stream = Filesystem.OpenRead(file);
            Read(stream);
        }

        /// <summary>
        /// Reads an ARC archive from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public void Read(Stream stream)
        {
            var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            _io = io;
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
                _entries.Add(entry);
                _entryByName[entry.FileName] = entry;
            }
        }

        public byte[] GetEntryNoHeader(ARCEntry entry)
        {
            _io.Seek(SeekOrigin.Begin, entry.FileOffset);
            var oldPosition = (int)_io.Position;
            _io.Skip(16);
            _io.ReadNullTerminatedString();
            _io.Skip(4);
            var offset = (int)_io.Position - oldPosition;
            return _io.ReadBytes((int)entry.FileSize - offset);
        }

        public byte[] GetEntry(ARCEntry entry)
        {
            _io.Seek(SeekOrigin.Begin, entry.FileOffset);

            return _io.ReadBytes((int)entry.FileSize);
        }

        public byte[] GetItemByName(string filename)
        {
            if (_entryByName.ContainsKey(filename))
                return GetEntryNoHeader(_entryByName[filename]);
            else
                return null;
        }

        public void Dispose()
        {
            _io.Dispose();
        }
    }
}
