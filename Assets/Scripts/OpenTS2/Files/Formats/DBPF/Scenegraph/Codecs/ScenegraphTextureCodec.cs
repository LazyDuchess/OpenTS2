using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Codecs
{
    [Codec(TypeIDs.SCENEGRAPH_TXTR)]
    public class ScenegraphTextureCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var image = ScenegraphResourceCollection.DeserializeSingletonScenegraphBlock<ImageDataBlock>(reader);
            return new ScenegraphTextureAsset(image);
        }
    }
}