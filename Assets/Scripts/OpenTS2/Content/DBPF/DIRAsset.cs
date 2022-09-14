using OpenTS2.Common;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.OpenTS2.Content.DBPF
{
    public class DIRAsset : AbstractAsset
    {
        public Dictionary<ResourceKey, uint> m_SizeByInternalTGI = new Dictionary<ResourceKey, uint>();
    }
}
