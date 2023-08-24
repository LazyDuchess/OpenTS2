using OpenTS2.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class SemiGlobalAsset : AbstractAsset
    {
        public string FileName;
        public string SemiGlobalGroupName;
        public uint SemiGlobalGroupID => FileUtils.GroupHash(SemiGlobalGroupName);
    }
}
