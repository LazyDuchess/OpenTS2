using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_TEXTURES)]
    public class LotTexturesCodec : AbstractCodec
    {
        private const uint TypeId = 0x4B58975B;

        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            var id = reader.ReadUInt32();
            if (id != TypeId)
            {
                throw new ArgumentException($"LTTX has wrong id {id:x}");
            }

            // Not known if other versions exist.
            var version = reader.ReadUInt32();
            Debug.Assert(version == 7, "Wrong version for LTTX");

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            string baseTexture = reader.ReadVariableLengthPascalString();

            int blendCount = reader.ReadInt32();

            string[] blendTextures = new string[blendCount];

            for (int i = 0; i < blendCount; i++)
            {
                blendTextures[i] = reader.ReadVariableLengthPascalString();
            }

            return new LotTexturesAsset(width, height, baseTexture, blendTextures);
        }
    }
}