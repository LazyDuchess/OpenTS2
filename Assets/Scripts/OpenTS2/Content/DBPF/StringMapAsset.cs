using System.Collections.Generic;

namespace OpenTS2.Content.DBPF
{
    public struct StringMapEntry
    {
        public string Value;
        public ushort Id;
        public ushort Unknown1;
        public ushort Unknown2;

        public override string ToString()
        {
            return $"{Value}: {Id} ({Unknown1} {Unknown2})";
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