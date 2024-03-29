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

        DiagonalLongGable = 10,
        DiagonalShortGable = 11,
        DiagonalHip = 12,
        DiagonalShedGable = 13,
        DiagonalShedHip = 14,

        GreenhouseLongGable = 15,

        PagodaHip = 16,
        PagodaLongGable = 17,
        PagodaShedGable = 18,
        DiagonalPagodaHip = 19,
        DiagonalPagodaLongGable = 20,
        DiagonalPagodaShedGable = 21
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