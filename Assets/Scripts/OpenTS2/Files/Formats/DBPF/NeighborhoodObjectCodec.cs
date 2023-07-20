using System;
using System.Text;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.NHOOD_OBJECT)]
    public class NeighborhoodObjectCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var propertySet = new PropertySet(Encoding.UTF8.GetString(bytes));

            var modelName = propertySet.GetProperty<StringProp>("modelname").Value;
            // Yup, the game just tacks on a _cres to the end of this.
            if (!modelName.EndsWith("_cres"))
            {
                modelName += "_cres";
            }

            return new NeighborhoodObjectAsset(
                modelName,
                propertySet.GetProperty<Uint32Prop>("guid").Value);
        }
    }
}