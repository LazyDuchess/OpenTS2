﻿using OpenTS2.Common;
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
        public AbstractCodec codec;
        public override byte[] bytes
        {
            get
            {
                return codec.Serialize(asset);
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public ChangedAsset(AbstractAsset asset, AbstractCodec codec)
        {
            this.asset = asset;
            this.codec = codec;
            this.entry = new DBPFEntry()
            {
                tgi = this.asset.tgi,
                internalTGI = this.asset.internalTGI,
                dynamic = true
            };
        }
        public ChangedAsset(AbstractAsset asset) : this(asset, Codecs.Get(asset.TGI.TypeID))
        {
        }
    }
}
