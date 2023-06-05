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
    // https://modthesims.info/wiki.php?title=NID

    [Codec(TypeIDs.NHOOD_INFO)]
    public class NeighborhoodInfoCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new NeighborhoodInfoAsset();
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);
            var version = reader.ReadUInt32();
            asset.MainPrefix = reader.ReadUint32PrefixedString();
            asset.ID = reader.ReadUInt32();
            if (version >= 5)
            {
                asset.NeighborhoodType = (Neighborhood.Type)reader.ReadUInt32();
                asset.SubPrefix = reader.ReadUint32PrefixedString();
            }
            reader.Dispose();
            stream.Dispose();
            return asset;
        }
    }
}
