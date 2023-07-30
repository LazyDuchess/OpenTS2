using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Codecs
{
    [Codec(TypeIDs.SCENEGRAPH_GMND)]
    public class ScenegraphGeometryNodeCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var scenegraphCollection = ScenegraphResourceCollection.Deserialize(reader);
            var geometryBlock = scenegraphCollection.GetBlockOfType<GeometryNodeBlock>();

            // Look up the GMDC that this GMND points to.
            Debug.Assert(geometryBlock.GeometryDataReference is ExternalReference);
            var gmdcRef = (ExternalReference)geometryBlock.GeometryDataReference;
            var gmdcKey = scenegraphCollection.FileLinks[gmdcRef.FileLinksIndex];

            return new ScenegraphGeometryNodeAsset(gmdcKey);
        }
    }
}