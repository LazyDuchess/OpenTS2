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
        public TGI tgi = TGI.Default;
        public string sourceFile = "";
        public abstract void Deserialize(byte[] bytes);
        public abstract AbstractAsset BuildAsset();

        /// <summary>
        /// Call on built asset after done constructing it to transfer information.
        /// </summary>
        /// <param name="asset">Built asset.</param>
        protected void PostBuildAsset(AbstractAsset asset)
        {
            asset.tgi = tgi;
            asset.sourceFile = sourceFile;
        }
    }
}
