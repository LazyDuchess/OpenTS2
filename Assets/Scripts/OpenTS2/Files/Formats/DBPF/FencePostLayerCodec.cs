using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_FENCEPOST)]
    public class FencePostLayerCodec : AbstractCodec
    {
        private const uint TypeId = 0xab4ba572;
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
                throw new ArgumentException($"FPST has wrong id {id:x}");
            }

            // Not known if other versions exist.
            var version = reader.ReadUInt32();
            Debug.Assert(version == Version, "Wrong version for FPST");

            string blockName = reader.ReadVariableLengthPascalString();

            if (blockName != "cFencePostLayer")
            {
                throw new ArgumentException($"FPST has wrong block name {blockName}");
            }

            int count = reader.ReadInt32();

            var entries = new FencePost[count];

            for (int i = 0; i < count; i++)
            {
                entries[i] = new FencePost()
                {
                    Level = reader.ReadInt32(),
                    XPos = reader.ReadFloat(),
                    YPos = reader.ReadFloat(),
                    GUID = reader.ReadUInt32()
                };
            }

            return new FencePostLayerAsset(entries);
        }
    }
}