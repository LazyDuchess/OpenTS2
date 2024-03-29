using System;
using System.Text;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.CATALOG_OBJECT)]
    public class CatalogObjectCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            try
            {
                var propertySet = new PropertySet(bytes);

                return new CatalogObjectAsset(propertySet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}