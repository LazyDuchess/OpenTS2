using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    public static class Codecs
    {
        static Dictionary<uint, Type> codecsByTypeID = new Dictionary<uint, Type>()
        {
            { Types.STR, typeof(STR) }
        };

        public static AbstractCodec GetCodecInstanceForType(uint type)
        {
            return (AbstractCodec)Activator.CreateInstance(codecsByTypeID[type]);
        }
    }
}
