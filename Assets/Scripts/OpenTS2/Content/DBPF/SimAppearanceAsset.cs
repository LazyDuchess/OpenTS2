using OpenTS2.Common;
using OpenTS2.Files.Formats.XML;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenTS2.Content.DBPF
{
    public class SimAppearanceAsset : AbstractAsset
    {
        public SimAppearanceAsset(PropertySet propertySet)
        {
            _properties = propertySet;
            EntryLists = ParseEntryLists(propertySet);
        }

        private readonly PropertySet _properties;

        /// <summary>
        /// How much the skeleton is stretched by.
        /// </summary>
        public float Stretch => float.Parse(_properties.GetProperty<StringProp>("stretch").Value);

        /// <summary>
        /// Each entry is either a resolved resource key, or an index into the corresponding ResourceKeyListAsset that
        /// must be resolved by the caller.
        /// </summary>
        public IReadOnlyDictionary<OutfitCategoryKey, List<IResourceKeyOrIndex>> EntryLists { get; }

        private static Dictionary<OutfitCategoryKey, List<IResourceKeyOrIndex>> ParseEntryLists(PropertySet properties)
        {
            var result = new Dictionary<OutfitCategoryKey, List<IResourceKeyOrIndex>>();

            if (!properties.Properties.TryGetValue("listcnt", out var listCntProp))
                return result;

            var listCount = PropertySet.ParsePropAsUint32(listCntProp);
            for (uint i = 0; i < listCount; i++)
            {
                var entryCount = PropertySet.ParsePropAsUint32(properties.Properties[$"ls{i}"]);
                var compositeKey = PropertySet.ParsePropAsUint32(properties.Properties[$"lk{i}"]);

                var entries = new List<IResourceKeyOrIndex>((int)entryCount);
                for (uint j = 0; j < entryCount; j++)
                {
                    entries.Add(ParseEntry(properties.Properties[$"le{i}_{j}"]));
                }

                result[OutfitCategoryKey.FromComposite(compositeKey)] = entries;
            }

            return result;
        }

        /// <summary>
        /// An entry is a direct "type:group:instance" hex triplet when no external key vector was used to
        /// write the resource, otherwise it's an index into that vector (the sidecar ResourceKeyListAsset).
        /// </summary>
        private static IResourceKeyOrIndex ParseEntry(IPropertyType property)
        {
            if (property is StringProp stringProp && stringProp.Value.Contains(":"))
            {
                var tokens = stringProp.Value.Split(':');
                var type = uint.Parse(tokens[0], NumberStyles.HexNumber);
                var group = uint.Parse(tokens[1], NumberStyles.HexNumber);
                var instance = uint.Parse(tokens[2], NumberStyles.HexNumber);
                return new ResourceKeyProp { Value = new ResourceKey(instance, group, type) };
            }

            return new ResourceKeyIndexProp { Value = (int)PropertySet.ParsePropAsUint32(property) };
        }
    }

    public interface IResourceKeyOrIndex
    {
    }

    public struct ResourceKeyProp : IResourceKeyOrIndex
    {
        public ResourceKey Value;
    }

    public struct ResourceKeyIndexProp : IResourceKeyOrIndex
    {
        public int Value;
    }

    [Flags]
    public enum OutfitType : uint
    {
        Casual1 = 0x1,
        Casual2 = 0x2,
        Casual3 = 0x4,
        Swim = 0x8,
        Sleep = 0x10,
        Formal = 0x20,
        Underwear = 0x40,
        Naked = 0x80,
        Maternity = 0x100,
        Gym = 0x200,
        TryOn = 0x400,
        NakedOverlay = 0x800,
        Outerwear = 0x1000,
    }

    /// <summary>
    /// The tail/ear entries are pet-only body parts.
    /// </summary>
    [Flags]
    public enum OutfitCategory : uint
    {
        Hair = 0x1,
        Face = 0x2,
        Top = 0x4,
        Body = 0x8,
        Bottom = 0x10,
        Accessory = 0x20,
        TailLong = 0x40,
        EarsUp = 0x80,
        TailShort = 0x100,
        EarsDown = 0x200,
        BrushTailLong = 0x400,
        BrushTailShort = 0x800,
        SpitzTail = 0x1000,
        BrushSpitzTail = 0x2000,
    }

    /// <summary>
    /// The composite "lk#" key: (outfitType << 16) | per-body-part category bitmask.
    /// </summary>
    public readonly struct OutfitCategoryKey : IEquatable<OutfitCategoryKey>
    {
        public readonly OutfitType OutfitType;
        public readonly OutfitCategory Category;

        public OutfitCategoryKey(OutfitType outfitType, OutfitCategory category)
        {
            OutfitType = outfitType;
            Category = category;
        }

        public static OutfitCategoryKey FromComposite(uint compositeKey)
        {
            return new OutfitCategoryKey((OutfitType)(compositeKey >> 16), (OutfitCategory)(compositeKey & 0xFFFF));
        }

        public bool Equals(OutfitCategoryKey other)
        {
            return OutfitType == other.OutfitType && Category == other.Category;
        }

        public override bool Equals(object obj)
        {
            return obj is OutfitCategoryKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            // Rider auto-generated impl.
            unchecked
            {
                return ((int)OutfitType * 397) ^ (int)Category;
            }
        }
    }
}
