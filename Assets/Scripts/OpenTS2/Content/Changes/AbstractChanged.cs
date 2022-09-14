using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.Changes
{
    public abstract class AbstractChanged
    {
        public virtual AbstractAsset asset { get; set; }
        public virtual byte[] bytes { get; set; }
        public DBPFEntry entry;
    }
}
