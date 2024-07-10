using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
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
            var neighbor = new NeighborAsset();
            neighbor.Id = (short)tgi.InstanceID;
            return neighbor;
        }
    }
}
