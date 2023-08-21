using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using System;
using System.IO;

namespace OpenTS2.Files.Formats.DBPF
{
    public struct Vector4<T> where T : unmanaged
    {
        public T x;
        public T y;
        public T z;
        public T w;

        public Vector4(T x, T y, T z, T w)
        {
            (this.x, this.y, this.z, this.w) = (x, y, z, w);
        }
        
        public override string ToString()
        {
            return $"{x}";
        }
    }

    [Codec(TypeIDs.LOT_3ARY)]
    public class _32ArrayCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            int id = reader.ReadInt32(); // Magic?
            int version = reader.ReadInt32(); // Version?
            string blockName = reader.ReadVariableLengthPascalString();

            if (blockName != "c3DArray")
            {
                throw new NotImplementedException($"Invalid 3D Array type {blockName}");
            }

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int depth = reader.ReadInt32();

            // Determine the data type using the remaining stream size.

            int elements = width * height * depth;
            int elemSize = (int)((stream.Length - stream.Position) / elements);

            Vector4<ushort> readVec4Ushort()
            {
                return new Vector4<ushort>(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());
            }

            Vector4<int> readVec4Int()
            {
                return new Vector4<int>(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
            }

            switch (elemSize)
            {
                case 1:
                    return new _3DArrayAsset<byte>(width, height, depth, id, ReadElements(elements, reader.ReadByte, depth));
                case 2:
                    return new _3DArrayAsset<ushort>(width, height, depth, id, ReadElements(elements, reader.ReadUInt16, depth));
                case 4:
                    return new _3DArrayAsset<float>(width, height, depth, id, ReadElements(elements, reader.ReadFloat, depth));
                case 8:
                    return new _3DArrayAsset<Vector4<ushort>>(width, height, depth, id, ReadElements(elements, readVec4Ushort, depth));
                case 16:
                    return new _3DArrayAsset<Vector4<int>>(width, height, depth, id, ReadElements(elements, readVec4Int, depth));
                default:
                    throw new NotImplementedException($"Invalid 3D Array size {elemSize}");
            }
        }

        private T[][] ReadElements<T>(int elements, Func<T> reader, int depth)
        {
            T[][] result = new T[depth][];

            for (int i = 0; i < depth; i++)
            {
                int elemSlice = elements / depth;
                T[] slice = new T[elements];

                for (int j = 0; j < elemSlice; j++)
                {
                    slice[j] = reader();
                }

                result[i] = slice;
            }

            return result;
        }
    }
}