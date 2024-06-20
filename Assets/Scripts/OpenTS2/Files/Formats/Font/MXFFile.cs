using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenTS2.Files.Formats.Font
{
    /// <summary>
    /// Decodes Maxis font files (*.mxf), usually a wrapper for *.ttf or *.pfb fonts.
    /// </summary>
    public class MXFFile
    {
        public byte[] DecodedData { get; private set; }

        public MXFFile(string filename)
        {
            var fstream = new FileStream(filename, FileMode.Open);
            var reader = new BinaryReader(fstream);

            var magic = reader.ReadBytes(4);
            if (magic[0] != 77 || magic[1] != 88 || magic[2] != 70 || magic[3] != 78)
                throw new IOException("Not a valid Maxis Font File!");

            reader.ReadInt32();
            var mask = reader.ReadByte();
            var len = fstream.Length;
            var memstream = new MemoryStream();
            var writer = new BinaryWriter(memstream);
            while (fstream.Position != len)
            {
                writer.Write((byte)(reader.ReadByte() ^ mask));
            }
            writer.Flush();
            DecodedData = memstream.ToArray();
            writer.Dispose();
            memstream.Dispose();
            reader.Dispose();
            fstream.Dispose();
        }
    }
}