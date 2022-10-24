using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class AssemblyAsset : AbstractAsset
    {
        public readonly byte[] data;
        public AssemblyAsset(byte[] data)
        {
            this.data = data;
        }
    }
}
