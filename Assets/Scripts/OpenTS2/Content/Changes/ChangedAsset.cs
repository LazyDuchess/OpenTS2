using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.Changes
{
    public class ChangedAsset : AbstractChanged
    {
        public override bool Compressed
        {
            get
            {
                return Asset.Compressed;
            }
        }
        public AbstractCodec Codec;
        public override byte[] Bytes
        {
            get
            {
                return Codec.Serialize(Asset);
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public ChangedAsset(AbstractAsset asset, AbstractCodec codec)
        {
            this.Asset = asset;
            this.Codec = codec;
            this.Entry = new DynamicDBPFEntry()
            {
                Dynamic = true,
                Change = this,
                Package = asset.Package
            };
        }
        public ChangedAsset(AbstractAsset asset) : this(asset, Codecs.Get(asset.GlobalTGI.TypeID))
        {
        }
    }
}
