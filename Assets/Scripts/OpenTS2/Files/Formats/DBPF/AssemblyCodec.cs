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
    [Codec(TypeIDs.DLL)]
    public class AssemblyCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            return new AssemblyAsset(bytes);
        }
    }
}
