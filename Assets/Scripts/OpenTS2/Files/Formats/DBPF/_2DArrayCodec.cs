using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_TERRAIN)]
    public class _2DArrayCodec : AbstractCodec
    {
        private const uint TypeId = 0x6b943b43;
        private const string TypeName = "c2DArray";

        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            var id = reader.ReadUInt32();
            if (id != TypeId)
            {
                throw new ArgumentException($"c2DArray has wrong id {id:x}");
            }

            var version = reader.ReadUInt32();
            Debug.Assert(version == 1, "Wrong version for c2DArray");

            string blockName = reader.ReadVariableLengthPascalString();

            if (blockName != TypeName)
            {
                throw new NotImplementedException($"Wrong type name {blockName}, expected c2DArray.");
            }

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            // Determine the data type using the remaining stream size.

            int elements = width * height;
            int elemSize = (int)((stream.Length - stream.Position) / elements);

            return new _2DArrayAsset(width, height, elemSize, reader.ReadBytes(elements * elemSize));
        }

        public override byte[] Serialize(AbstractAsset asset)
        {
            var arrayAsset = asset as _2DArrayAsset;
            arrayAsset.AutoCommit();

            var stream = new MemoryStream(0);
            var writer = new BinaryWriter(stream);
            writer.Write(new byte[64]);
            writer.Write(TypeId);
            writer.Write(1);
            writer.Write(TypeName.Length);
            writer.Write(Encoding.UTF8.GetBytes(TypeName));
            writer.Write(arrayAsset.Width);
            writer.Write(arrayAsset.Height);
            writer.Write(arrayAsset.Data);

            var buffer = StreamUtils.GetBuffer(stream);
            stream.Dispose();
            writer.Dispose();
            return buffer;
        }
    }
}