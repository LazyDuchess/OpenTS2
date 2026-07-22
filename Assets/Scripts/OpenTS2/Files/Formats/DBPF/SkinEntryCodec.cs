using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.SKIN_ENTRY)]
    public class SkinEntryCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var propertySet = new PropertySet(bytes);
            return new SkinEntryAsset(propertySet);
        }
    }
}
