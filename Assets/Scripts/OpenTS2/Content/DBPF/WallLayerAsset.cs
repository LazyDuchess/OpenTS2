using System.Collections.Generic;

namespace OpenTS2.Content.DBPF
{
    public struct WallLayerEntry
    {
        public int Id;
        public int WallType; // 1/3?
        public ushort Pattern1;
        public ushort Pattern2;

        public override string ToString()
        {
            return $"{Id}: {WallType} w/ patterns: {Pattern1} {Pattern2}";
        }
    }

    public class WallLayerAsset : AbstractAsset
    {
        public Dictionary<int, WallLayerEntry> Walls { get; }

        public WallLayerAsset(WallLayerEntry[] walls)
        {
            Walls = ToLayerDictionary(walls);
        }

        private Dictionary<int, WallLayerEntry> ToLayerDictionary(WallLayerEntry[] pos)
        {
            var result = new Dictionary<int, WallLayerEntry>(pos.Length);

            foreach (var position in pos)
            {
                result[position.Id] = position;
            }

            return result;
        }
    }

}