using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using System;
using System.IO;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_TERRAIN)]
    public class _2DArrayCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            int id = reader.ReadInt32(); // Magic?
            int version = reader.ReadInt32(); // Version
            string blockName = reader.ReadVariableLengthPascalString();

            if (blockName != "c2DArray")
            {
                throw new NotImplementedException($"Invalid 2D Array type {blockName}");
            }

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            // Determine the data type using the remaining stream size.

            int elements = width * height;
            int elemSize = (int)((stream.Length - stream.Position) / elements);

            switch (elemSize)
            {
                case 1:
                    return new _2DArrayAsset<byte>(width, height, id, ReadElements(elements, reader.ReadByte));
                case 2:
                    return new _2DArrayAsset<ushort>(width, height, id, ReadElements(elements, reader.ReadUInt16));
                case 4:
                    return new _2DArrayAsset<float>(width, height, id, ReadElements(elements, reader.ReadFloat));
                default:
                    throw new NotImplementedException($"Invalid 2D Array size {elemSize}");
            }
        }

        private T[] ReadElements<T>(int elements, Func<T> reader)
        {
            T[] result = new T[elements];

            for (int i = 0; i < elements; i++)
            {
                result[i] = reader();
            }

            return result;
        }
    }
}