using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class BaseLotInfoAsset : AbstractAsset
    {
        public string Filename { get; }
        public BaseLotInfo BaseLotInfo { get; }

        public BaseLotInfoAsset(string fileName, BaseLotInfo baseLotInfo)
        {
            Filename = fileName;
            BaseLotInfo = baseLotInfo;
        }
    }
}
