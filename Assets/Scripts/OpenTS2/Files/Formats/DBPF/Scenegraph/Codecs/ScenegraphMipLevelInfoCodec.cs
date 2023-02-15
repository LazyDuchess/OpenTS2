using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Utils;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Codecs
{
    [Codec(TypeIDs.SCENEGRAPH_LIFO)]
    public class ScenegraphMipLevelInfoCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var mip = ScenegraphResourceCollection.DeserializeSingletonScenegraphBlock<MipLevelInfoBlock>(reader);
            return new ScenegraphMipLevelInfoAsset(mip.Data);
        }
    }
}