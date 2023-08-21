using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Content.DBPF
{
    public static class CatalogObjectType
    {
        public const string Floor = "floor";
        public const string Wall = "wall";
    }

    public class CatalogObjectAsset : AbstractAsset
    {
        /// <summary>
        /// Contents can vary based on type.
        /// </summary>
        public PropertySet Properties { get; }

        /// <summary>
        /// The global unique id for the object.
        /// </summary>
        public uint Guid { get; }

        /// <summary>
        /// Attached material for walls/floors.
        /// </summary>
        public string Material => Properties.GetProperty<StringProp>("material").Value;

        /// <summary>
        /// Type of catalog object. See CatalogObjectType.
        /// </summary>
        public string Type => Properties.GetProperty<StringProp>("type").Value;

        public CatalogObjectAsset(PropertySet propertySet)
        {
            Properties = propertySet;
            Guid = propertySet.GetProperty<Uint32Prop>("guid").Value;
        }
    }
}