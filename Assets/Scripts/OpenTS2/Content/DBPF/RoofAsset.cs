namespace OpenTS2.Content.DBPF
{
    public enum RoofType
    {
        ShedGabled = 1,
        ShedHipped = 2,
        ShortGable = 3,
        LongGable = 4,
        Hip = 5,
        Mansard = 6, // This becomes Hip if the roof is too small?
        Cone = 7,
        Dome = 8,
        Octogonal = 9,

        // Diagonals next.

        // DiagonalLongGable

        // Then all the stupid ones

        // Straight roofs with the same slope support:
        // - Combined gabled edge that can make a jagged appearing line (cuts out lines that go under other roofs)
        //
        // Straight/diagonal roofs with the same slope support:
        // - Pretty intersection edges (but not between diag and straight)
        //
        // All other roofs don't really interact with each other.
    }

    public struct RoofEntry
    {
        public int Id;
        public float XFrom;
        public float YFrom;
        public int LevelFrom;
        public float XTo;
        public float YTo;
        public int LevelTo;
        public RoofType Type;
        public uint Pattern;

        // Version 2
        public float RoofAngle;

        // Version 3
        public int RoofStyleExtended;
    }

    public class RoofAsset : AbstractAsset
    {
        public RoofEntry[] Entries { get; }

        public RoofAsset(RoofEntry[] entries)
        {
            Entries = entries;
        }
    }

}