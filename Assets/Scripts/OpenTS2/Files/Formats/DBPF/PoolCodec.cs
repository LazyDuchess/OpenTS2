using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_POOL)]
    public class PoolCodec : AbstractCodec
    {
        private const uint TypeId = 0x0c900fdb;
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
                throw new ArgumentException($"POOL has wrong id {id:x}");
            }

            // Not known if other versions exist.
            var version = reader.ReadUInt32();
            Debug.Assert(version == Version, "Wrong version for POOL");

            int count = reader.ReadInt32();

            var entries = new PoolEntry[count];

            for (int i = 0; i < count; i++)
            {
                entries[i] = new PoolEntry()
                {
                    Unknown1 = reader.ReadInt32(),
                    XPos = reader.ReadFloat(),
                    YPos = reader.ReadFloat(),
                    Level = reader.ReadInt32(),
                    XSize = reader.ReadInt32(),
                    YSize = reader.ReadInt32(),
                    Unknown2 = reader.ReadInt32(),
                    YOffset = reader.ReadFloat(),
                    Unknown3 = reader.ReadInt32()
                };
            }

            return new PoolAsset(entries);
        }
    }
}