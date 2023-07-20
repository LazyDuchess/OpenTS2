using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace OpenTS2.Files.Formats.XML
{
    /// <summary>
    /// A cGZPropertySetString element that stores strings keys and arbitrary values in xml files.
    /// </summary>
    public class PropertySet
    {
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

        public PropertySet(string xml)
        {
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