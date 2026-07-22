using System;
using OpenTS2.Common;
using OpenTS2.Files.Formats.XML;

namespace OpenTS2.Content.DBPF
{
    public class SkinEntryAsset : AbstractAsset
    {
        public SkinEntryAsset(PropertySet propertySet)
        {
            ShapeResourceKey = ParseKeyOrIndex(propertySet, "shape");
        }

        public IResourceKeyOrIndex ShapeResourceKey { get; }

        private static IResourceKeyOrIndex ParseKeyOrIndex(PropertySet properties, string prefix)
        {
            // cTSSkinEntry::ReadProperties gates on "resourcerestypeid" for whether the whole record
            // (both its "resource" and "shape" keys) was written directly or via the external key vector.
            if (properties.Properties.ContainsKey("resourcerestypeid"))
            {
                var type = PropertySet.ParsePropAsUint32(properties.Properties[$"{prefix}restypeid"]);
                var group = PropertySet.ParsePropAsUint32(properties.Properties[$"{prefix}groupid"]);
                var instance = PropertySet.ParsePropAsUint32(properties.Properties[$"{prefix}id"]);
                return new ResourceKeyProp { Value = new ResourceKey(instance, group, type) };
            }

            return new ResourceKeyIndexProp { Value = (int)PropertySet.ParsePropAsUint32(properties.Properties[$"{prefix}keyidx"]) };
        }
    }
}
