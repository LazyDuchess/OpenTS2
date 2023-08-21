using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_WALLGRAPH)]
    public class WallGraphCodec : AbstractCodec
    {
        private const uint TypeId = 0x0a284d0b;
        private const int Version = 2;

        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            var id = reader.ReadUInt32();
            if (id != TypeId)
            {
                throw new ArgumentException($"WGRA has wrong id {id:x}");
            }

            // Not known if other versions exist.
            var version = reader.ReadUInt32();
            Debug.Assert(version >= Version, "Wrong version for WGRA");

            string blockName = reader.ReadVariableLengthPascalString();

            if (blockName != "cWallGraph")
            {
                throw new ArgumentException($"WGRA has wrong block name {blockName}");
            }

            int unk1 = reader.ReadInt32();
            int unk2 = reader.ReadInt32();
            int baseFloor = reader.ReadInt32();

            if (unk1 != 0 || unk2 != 0)
            {
                throw new ArgumentException($"Unexpected WGRA header, should be all 0");
            }

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int floors = reader.ReadInt32();

            // Not sure what these IDs are for. The second is generally higher than the first.
            int id1 = reader.ReadInt32();
            int id2 = reader.ReadInt32();
            int posCount = reader.ReadInt32();

            var pos = new WallGraphPositionEntry[posCount];

            for (int i = 0; i < posCount; i++)
            {
                pos[i] = new WallGraphPositionEntry()
                {
                    Id = reader.ReadInt32(),
                    XPos = reader.ReadFloat(),
                    YPos = reader.ReadFloat(),
                    Level = reader.ReadInt32(),
                };
            }

            int idCount = reader.ReadInt32();
            int[] rooms = new int[idCount];

            for (int i = 0; i < idCount; i++)
            {
                rooms[i] = reader.ReadInt32();
            }

            int lineCount = reader.ReadInt32();

            var lines = new WallGraphLineEntry[lineCount];

            for (int i = 0; i < lineCount; i++)
            {
                lines[i] = new WallGraphLineEntry()
                {
                    LayerId = reader.ReadInt32(),
                    FromId = reader.ReadInt32(),
                    Room1 = reader.ReadInt32(),
                    ToId = reader.ReadInt32(),
                    Room2 = reader.ReadInt32()
                };
            }

            return new WallGraphAsset(width, height, floors, baseFloor, pos, rooms, lines);
        }
    }
}