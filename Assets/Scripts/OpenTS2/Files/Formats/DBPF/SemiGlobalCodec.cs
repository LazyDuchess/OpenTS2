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
    [Codec(TypeIDs.SEMIGLOBAL)]
    public class SemiGlobalCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var asset = new SemiGlobalAsset();

            // Just the resource filename.
            asset.FileName = reader.ReadNullTerminatedUTF8();
            reader.Seek(SeekOrigin.Begin, 64);

            // The actual semiglobal name.
            asset.SemiGlobalGroupName = reader.ReadBytePrefixedString();

            return asset;
        }
    }
}
