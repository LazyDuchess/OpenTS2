using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    public class DynamicDBPFEntry : DBPFEntry
    {
        public ChangedResourceMetadata Change = new ChangedResourceMetadata();
        public override uint FileSize
        {
            get
            {
                return Change.Data.FileSize;
            }
        }
        public override byte[] GetBytes()
        {
            return Change.Data.GetBytes();
        }

        public override AbstractAsset GetAsset()
        {
            return Change.Data.GetAsset();
        }
    }
}
