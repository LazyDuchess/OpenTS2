namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// Contains information about a lot in a neighborhood such as its type, road connections, position and elevation.
    /// </summary>
    public class LotInfoAsset : AbstractAsset
    {
        public int LotId { get; }
        public uint Flags { get; }
        public byte LotType { get; }
        public string LotName { get; }
        public string LotDescription { get; }
        public uint Width { get; }
        public uint Depth { get; }
        public byte RoadsAlongEdges { get; }
        public uint LocationX { get; }
        public uint LocationY { get; }
        public float NeighborhoodToLotHeightOffset { get; }
        public byte FrontEdge { get; }

        public byte CreationFrontEdge { get; }

        public LotInfoAsset(int lotId, uint flags, byte lotType, string lotName, string lotDescription,
            uint width, uint depth, byte roadsAlongEdges, uint locationX, uint locationY, float neighborhoodToLotHeightOffset,
            byte creationFrontEdge, byte frontEdge)
        {
            LotId = lotId;
            Flags = flags;
            LotType = lotType;
            LotName = lotName;
            LotDescription = lotDescription;
            Width = width;
            Depth = depth;
            RoadsAlongEdges = roadsAlongEdges;
            LocationX = locationX;
            LocationY = locationY;
            NeighborhoodToLotHeightOffset = neighborhoodToLotHeightOffset;
            CreationFrontEdge = creationFrontEdge;
            FrontEdge = frontEdge;
        }

        public float WorldLocationX => LocationX * NeighborhoodTerrainAsset.TerrainGridSize;
        public float WorldLocationY => LocationY * NeighborhoodTerrainAsset.TerrainGridSize;
        public float WorldWidth => Width * NeighborhoodTerrainAsset.TerrainGridSize;
        public float WorldDepth => Depth * NeighborhoodTerrainAsset.TerrainGridSize;

        public bool HasRoadAlongEdge(LotEdge edge)
        {
            return ((1 << (int)edge) & RoadsAlongEdges) != 0;
        }
    }

    /// <summary>
    /// The different edges of a lot. Used to check for if roads are present, names are in unity space.
    /// </summary>
    public enum LotEdge
    {
        NegativeX = 0,
        PositiveZ = 1,
        PositiveX = 2,
        NegativeZ = 3,
    }
}