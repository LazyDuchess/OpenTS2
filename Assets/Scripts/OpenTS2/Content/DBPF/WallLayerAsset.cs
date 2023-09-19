using System.Collections.Generic;

namespace OpenTS2.Content.DBPF
{
    public enum WallType : int
    {
        Normal = 1,
        ThinFence = 2, // unused?
        Roof = 3,
        DeckInvis = 4,
        Deck = 16,
        Foundation = 23,
        Deck2 = 24,
        Deck3 = 26,
        Pool = 29,
        OFBWall = 300,
        OFBScreen = 301
    }

    public struct WallLayerEntry
    {
        public int Id;
        public WallType WallType;
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