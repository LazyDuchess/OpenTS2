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
    /// Returns data from an entry for a changed DBPF asset.
    /// </summary>
    public class ChangedResourceDataEntry : ChangedResourceData
    {
        private DBPFEntry _dbpfEntry;

        public override uint FileSize => _dbpfEntry.FileSize;

        public ChangedResourceDataEntry(DBPFEntry entry)
        {
            this._dbpfEntry = entry;
        }
        public override byte[] GetBytes()
        {
            return _dbpfEntry.GetBytes();
        }

        public override AbstractAsset GetAsset()
        {
            return _dbpfEntry.GetAsset();
        }
    }
}
