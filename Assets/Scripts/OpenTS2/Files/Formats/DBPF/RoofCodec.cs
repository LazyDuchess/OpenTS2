using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_ROOF)]
    public class RoofCodec : AbstractCodec
    {
        private const uint TypeId = 0xab9406aa;
        private const int VersionMin = 1;
        private const int VersionMax = 3;

        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            var id = reader.ReadUInt32();
            if (id != TypeId)
            {
                throw new ArgumentException($"ROOF has wrong id {id:x}");
            }

            // Not known if other versions exist.
            var version = reader.ReadUInt32();
            Debug.Assert(version >= VersionMin || version <= VersionMax, "Wrong version for ROOF");

            int count = reader.ReadInt32();

            var entries = new RoofEntry[count];

            for (int i = 0; i < count; i++)
            {
                entries[i] = new RoofEntry()
                {
                    Id = reader.ReadInt32(),
                    XFrom = reader.ReadFloat(),
                    YFrom = reader.ReadFloat(),
                    LevelFrom = reader.ReadInt32(),
                    XTo = reader.ReadFloat(),
                    YTo = reader.ReadFloat(),
                    LevelTo = reader.ReadInt32(),
                    Type = (RoofType)reader.ReadInt32(),
                    Pattern = reader.ReadInt32(),

                    RoofAngle = version >= 2 ? reader.ReadFloat() : 0.5f,
                    RoofStyleExtended = version >= 3 ? reader.ReadInt32() : 0 // 3 for gable roof on an example?
                };
            }

            return new RoofAsset(entries);
        }
    }
}