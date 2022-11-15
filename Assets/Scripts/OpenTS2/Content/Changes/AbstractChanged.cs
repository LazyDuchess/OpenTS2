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
        public virtual bool Compressed { get; set; } = false;
        public virtual AbstractAsset Asset { get; set; }
        public virtual byte[] Bytes { get; set; }
        public DBPFEntry Entry;
    }
}
