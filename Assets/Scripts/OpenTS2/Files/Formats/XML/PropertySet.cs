using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace OpenTS2.Files.Formats.XML
{
    /// <summary>
    /// A cGZPropertySetString element that stores strings keys and arbitrary values in xml files.
    /// </summary>
    public class PropertySet
    {
        private enum CPFType : uint
        {
            Int = 0xEB61E4F7,
            String = 0x0B8BEA18,
            Float = 0xABC78708,
            Bool = 0xCBA908E1,
            Int2 = 0x0C264712
        }

        public Dictionary<string, IPropertyType> Properties { get; } = new Dictionary<string, IPropertyType>();

        /// <summary>
        /// Gets a property of a particular type. Throws if type does not match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetProperty<T>(string key) where T: IPropertyType
        {
            if (Properties[key] is T type)
            {
                return type;
            }

            throw new ArgumentException($"{key} is of type {Properties[key].GetType()}, not {typeof(T)}");
        }

        public PropertySet(byte[] data)
        {
            if (data[0] == 0xe0 && data[1] == 0x50 && data[2] == 0xe7 && data[3] == 0xcb)
            {
                var stream = new MemoryStream(data);
                var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

                LoadFromCPF(reader);
            }
            else
            {
                LoadFromXML(Encoding.UTF8.GetString(data));
            }
        }

        public PropertySet(string xml)
        {
            LoadFromXML(xml);
        }

        private void LoadFromCPF(IoBuffer reader)
        {
            uint type = reader.ReadUInt32();
            short version = reader.ReadInt16();

            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                CPFType fieldType = (CPFType)reader.ReadUInt32();
                string key = reader.ReadUint32PrefixedString();

                switch (fieldType)
                {
                    case CPFType.Int:
                    case CPFType.Int2:
                        Properties[key] = new Uint32Prop() { Value = reader.ReadUInt32() };
                        break;
                    case CPFType.Float:
                        Properties[key] = new StringProp() { Value = reader.ReadSingle().ToString(CultureInfo.InvariantCulture) };
                        break;
                    case CPFType.Bool:
                        Properties[key] = new Uint32Prop() { Value = reader.ReadByte() };
                        break;
                    case CPFType.String:
                        Properties[key] = new StringProp() { Value = reader.ReadUint32PrefixedString() };
                        break;
                }
            }
        }

        private void LoadFromXML(string xml)
        {
            // Special characters are used without proper escaping in XMLs in TS2. & needs escaped.
            xml = xml.Replace("&", "&amp;");

            var parsed = XElement.Parse(xml);
            if (parsed.Name.LocalName != "cGZPropertySetString")
            {
                throw new ArgumentException("cGZPropertySetString not in xml");
            }

            foreach (var property in parsed.Elements())
            {
                var key = property.Attribute("key")?.Value;
                if (key == null)
                    throw new ArgumentException("Property with no key attribute: " + property);

                var innerText = string.Concat(property.Nodes());
                IPropertyType value = property.Name.LocalName switch
                {
                    "AnyString" => new StringProp { Value = innerText },
                    "AnyUint32" => new Uint32Prop { Value = uint.Parse(innerText) },
                    _ => null
                };
                Properties[key] = value;
            }
        }
    }

    public interface IPropertyType
    {
    }

    public struct StringProp : IPropertyType
    {
        public string Value;
    }

    public struct Uint32Prop : IPropertyType
    {
        public uint Value;
    }
}