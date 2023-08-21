using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Content.DBPF
{
    public static class CatalogFenceType
    {
        public const string Fence = "fence";
    }

    public class CatalogFenceAsset : AbstractAsset
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
        /// Type of catalog object. See CatalogFenceType.
        /// </summary>
        public string Type => Properties.GetProperty<StringProp>("type").Value;

        /// <summary>
        /// Diagonal rail resource. (optional)
        /// </summary>
        public string DiagRail => Properties.Properties.TryGetValue("diagrail", out IPropertyType prop) ? ((StringProp)prop).Value : null;

        /// <summary>
        /// Rail resource.
        /// </summary>
        public string Rail => Properties.GetProperty<StringProp>("rail").Value;

        /// <summary>
        /// Post resource.
        /// </summary>
        public string Post => Properties.GetProperty<StringProp>("post").Value;

        public CatalogFenceAsset(PropertySet propertySet)
        {
            Properties = propertySet;
            Guid = propertySet.GetProperty<Uint32Prop>("guid").Value;
        }
    }
}