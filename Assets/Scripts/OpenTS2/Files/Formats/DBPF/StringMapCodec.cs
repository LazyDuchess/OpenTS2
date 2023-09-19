using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_STRINGMAP)]
    public class StringMapCodec : AbstractCodec
    {
        private const uint TypeId = 0xcac4fc40;
        private const int Version = 1;

        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            var id = reader.ReadUInt32();
            if (id != TypeId)
            {
                throw new ArgumentException($"SMAP has wrong id {id:x}");
            }

            // Not known if other versions exist.
            var version = reader.ReadUInt32();
            Debug.Assert(version == Version, "Wrong version for SMAP");

            string blockName = reader.ReadVariableLengthPascalString();

            if (blockName != "cStringMap")
            {
                throw new ArgumentException($"SMAP has wrong block name {blockName}");
            }

            int count = reader.ReadInt32();

            var entries = new StringMapEntry[count];

            for (int i = 0; i < count; i++)
            {
                entries[i] = new StringMapEntry()
                {
                    Value = reader.ReadVariableLengthPascalString(),
                    Id = reader.ReadUInt16(),
                    Unknown = reader.ReadUInt32()
                };
            }

            return new StringMapAsset(entries);
        }
    }
}