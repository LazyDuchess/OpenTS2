using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
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
    public class _3DArrayCodec : AbstractCodec
    {
        private const uint TypeId = 0x2a51171b;
        private const string TypeName = "c3DArray";

        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            var id = reader.ReadUInt32();
            if (id != TypeId)
            {
                throw new ArgumentException($"c3DArray has wrong id {id:x}");
            }

            var version = reader.ReadUInt32();
            Debug.Assert(version == 1, "Wrong version for c3DArray");

            string blockName = reader.ReadVariableLengthPascalString();

            if (blockName != TypeName)
            {
                throw new NotImplementedException($"Wrong type name {blockName}, expected c3DArray.");
            }

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int depth = reader.ReadInt32();

            // Determine the data type using the remaining stream size.

            int elements = width * height * depth;

            if (elements == 0)
            {
                return new _3DArrayAsset(width, height, depth, 0, new byte[0][]);
            }

            int elemSize = (int)((stream.Length - stream.Position) / elements);

            byte[][] result = new byte[depth][];

            for (int i = 0; i < depth; i++)
            {
                result[i] = reader.ReadBytes(width * height * elemSize);
            }

            return new _3DArrayAsset(width, height, depth, elemSize, result);
        }

        public override byte[] Serialize(AbstractAsset asset)
        {
            var arrayAsset = asset as _3DArrayAsset;
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
            writer.Write(arrayAsset.Depth);

            for (var i = 0; i < arrayAsset.Depth; i++)
            {
                writer.Write(arrayAsset.Data[i]);
            }

            var buffer = StreamUtils.GetBuffer(stream);
            stream.Dispose();
            writer.Dispose();
            return buffer;
        }
    }
}