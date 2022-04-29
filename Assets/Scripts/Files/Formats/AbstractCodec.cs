using OpenTS2.Common;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats
{
    public abstract class AbstractCodec
    {
        public abstract AbstractAsset Deserialize(byte[] bytes, TGI tgi, string sourceFile);
    }
}
