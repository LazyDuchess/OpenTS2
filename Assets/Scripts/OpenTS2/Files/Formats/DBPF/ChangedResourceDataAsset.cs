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
    /// Holds the data for a changed DBPF Asset.
    /// </summary>
    public class ChangedResourceDataAsset : ChangedResourceData
    {
        private AbstractAsset _asset;
        private AbstractCodec _codec;
        public ChangedResourceDataAsset(AbstractAsset asset, AbstractCodec codec = null)
        {
            this._asset = asset;
            this._codec = codec;
        }
        public override byte[] GetBytes()
        {
            if (_codec == null)
                return null;
            return _codec.Serialize(_asset);
        }
        public override AbstractAsset GetAsset()
        {
            return _asset;
        }
        public override uint FileSize => (uint)GetBytes().Length;
    }
}
