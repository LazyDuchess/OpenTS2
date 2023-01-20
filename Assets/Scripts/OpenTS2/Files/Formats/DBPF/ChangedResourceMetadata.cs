using OpenTS2.Common;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Holds the metadata of a changed DBPF Resource.
    /// </summary>
    public class ChangedResourceMetadata
    {
        public ChangedResourceData Data = null;
        public bool Compressed = false;
        public ChangedResourceMetadata()
        {

        }
    }
}
