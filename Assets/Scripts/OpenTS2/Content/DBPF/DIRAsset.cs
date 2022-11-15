using OpenTS2.Common;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class DIRAsset : AbstractAsset
    {
        public Dictionary<ResourceKey, uint> SizeByInternalTGI = new Dictionary<ResourceKey, uint>();
        public uint GetUncompressedSize(ResourceKey tgi)
        {
            if (SizeByInternalTGI.ContainsKey(tgi))
                return SizeByInternalTGI[tgi];
            return 0;
        }
    }
}
