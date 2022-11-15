using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class AssemblyAsset : AbstractAsset
    {
        public readonly byte[] Data;
        public AssemblyAsset(byte[] data)
        {
            this.Data = data;
        }
    }
}
