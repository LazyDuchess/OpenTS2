using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.NEIGHBOR)]
    public class NeighborCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);
            ms.Seek(0x1DC, SeekOrigin.Begin);
            var guid = reader.ReadUInt32();
            var neighborId = (short)tgi.InstanceID;
            var neighbor = new NeighborAsset((short)tgi.InstanceID, guid);
            return neighbor;
        }
    }
}
