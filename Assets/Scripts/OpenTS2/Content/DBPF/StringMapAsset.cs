using System.Collections.Generic;

namespace OpenTS2.Content.DBPF
{
    public struct StringMapEntry
    {
        public string Value;
        public ushort Id;
        public uint Unknown;

        public override string ToString()
        {
            return $"{Value}: {Id} ({Unknown})";
        }
    }

    public class StringMapAsset : AbstractAsset
    {
        public Dictionary<ushort, StringMapEntry> Map { get; }

        public StringMapAsset(StringMapEntry[] entries)
        {
            Map = new Dictionary<ushort, StringMapEntry>(entries.Length);

            for (int i = 0; i < entries.Length; i++)
            {
                Map.Add(entries[i].Id, entries[i]);
            }
        }
    }

}