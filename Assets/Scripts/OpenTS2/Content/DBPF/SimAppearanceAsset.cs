using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Content.DBPF
{
    public class SimAppearanceAsset : AbstractAsset
    {
        public SimAppearanceAsset(PropertySet propertySet)
        {
            _properties = propertySet;
        }

        private readonly PropertySet _properties;

        /// <summary>
        /// How much the skeleton is stretched by.
        /// </summary>
        public float Stretch => float.Parse(_properties.GetProperty<StringProp>("stretch").Value);
    }
}