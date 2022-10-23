using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    public class CodecAttribute : Attribute
    {
        public readonly uint[] TypeIDs;
        public CodecAttribute(params uint[] TypeIDs)
        {
            this.TypeIDs = TypeIDs;
        }
    }
}
