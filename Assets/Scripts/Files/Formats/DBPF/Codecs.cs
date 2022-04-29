using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    public static class Codecs
    {
        static Dictionary<uint, AbstractCodec> codecsByTypeID = new Dictionary<uint, AbstractCodec>()
        {
            { Types.STR, new STR() }
        };

        public static AbstractCodec GetCodecInstanceForType(uint type)
        {
            return codecsByTypeID[type];
        }
    }
}
