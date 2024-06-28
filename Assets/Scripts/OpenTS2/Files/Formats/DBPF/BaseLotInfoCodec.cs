using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Content;
using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.VersionControl;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.BASE_LOT_INFO)]
    public class BaseLotInfoCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var filename = reader.ReadNullTerminatedUTF8();
            reader.Seek(SeekOrigin.Begin, 64);

            // Inside the lotInfo is a nested baseLotInfo that carries information about the original lot.
            var baseLotInfo = new BaseLotInfo();
            baseLotInfo.Read(reader, false);

            return new BaseLotInfoAsset(filename, baseLotInfo);
        }
    }
}
