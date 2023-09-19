using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Content.DBPF
{
    public static class CatalogRoofType
    {
        public const string Roof = "roof";
    }

    public class CatalogRoofAsset : AbstractAsset
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
        /// Roof top bumpmap texture.
        /// </summary>
        public string TextureTopBump => Properties.Properties.TryGetValue("tecturetopbump", out IPropertyType prop) ? ((StringProp)prop).Value : null;

        /// <summary>
        /// Roof top texture.
        /// </summary>
        public string TextureTop => Properties.GetProperty<StringProp>("texturetop").Value;

        /// <summary>
        /// Roof edge texture.
        /// </summary>
        public string TextureEdges => Properties.GetProperty<StringProp>("textureedges").Value;

        /// <summary>
        /// Roof trim texture.
        /// </summary>
        public string TextureTrim => Properties.GetProperty<StringProp>("texturetrim").Value;

        /// <summary>
        /// Roof under texture.
        /// </summary>
        public string TextureUnder => Properties.GetProperty<StringProp>("textureunder").Value;

        public CatalogRoofAsset(PropertySet propertySet)
        {
            Properties = propertySet;
            Guid = propertySet.GetProperty<Uint32Prop>("guid").Value;
        }
    }
}