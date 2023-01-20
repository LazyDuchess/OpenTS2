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
    /// Holds the bytes for a changed DBPF Asset.
    /// </summary>
    public class ChangedResourceDataBytes : ChangedResourceData
    {
        private byte[] _bytes;
        private AbstractCodec _codec;
        private ResourceKey _tgi;
        private DBPFFile _package;
        public ChangedResourceDataBytes(byte[] bytes, ResourceKey tgi, AbstractCodec codec = null, DBPFFile package = null)
        {
            this._bytes = bytes;
            this._tgi = tgi;
            this._codec = codec;
            this._package = package;
        }
        public override byte[] GetBytes()
        {
            return _bytes;
        }
        public override AbstractAsset GetAsset()
        {
            if (_codec == null)
                return null;
            return _codec.Deserialize(_bytes, _tgi, _package);
        }
        public override uint FileSize => (uint)GetBytes().Length;
    }
}
