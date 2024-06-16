using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.OBJF)]
    public class OBJFCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new ObjectFunctionsAsset();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            asset.FileName = reader.ReadNullTerminatedUTF8();

            reader.Seek(SeekOrigin.Begin, 64);

            var unk = reader.ReadBytes(8);

            var magic = reader.ReadBytes(4);

            if (magic[0] != 'f' || magic[1] != 'J' || magic[2] != 'B' || magic[3] != 'O')
                throw new IOException("Invalid magic for OBJF resource");

            var entryAmount = reader.ReadUInt32();

            asset.Functions = new ObjectFunction[entryAmount];

            for(var i=0;i<entryAmount;i++)
            {
                var checkTree = reader.ReadUInt16();
                var action = reader.ReadUInt16();
                var func = new ObjectFunction(action, checkTree);

                asset.Functions[i] = func;
            }

            stream.Dispose();
            reader.Dispose();

            return asset;
        }
    }
}
