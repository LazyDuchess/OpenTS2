namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// Contains information about a lot in a neighborhood such as its type, road connections, position and elevation.
    /// </summary>
    public class LotInfoAsset : AbstractAsset
    {
        public BaseLotInfo BaseLotInfo { get; }
        public int LotId { get; }
        public uint LocationX { get; }
        public uint LocationY { get; }
        public float NeighborhoodToLotHeightOffset { get; }
        public byte FrontEdge { get; }

        public LotInfoAsset(BaseLotInfo baseLotInfo, int lotId, uint locationX, uint locationY, float neighborhoodToLotHeightOffset, byte frontEdge)
        {
            BaseLotInfo = baseLotInfo;
            LotId = lotId;
            LocationX = locationX;
            LocationY = locationY;
            NeighborhoodToLotHeightOffset = neighborhoodToLotHeightOffset;
            FrontEdge = frontEdge;
        }

        public float WorldLocationX => LocationX * NeighborhoodTerrainAsset.TerrainGridSize;
        public float WorldLocationY => LocationY * NeighborhoodTerrainAsset.TerrainGridSize;
        public float WorldWidth => BaseLotInfo.Width * NeighborhoodTerrainAsset.TerrainGridSize;
        public float WorldDepth => BaseLotInfo.Depth * NeighborhoodTerrainAsset.TerrainGridSize;

        public bool HasRoadAlongEdge(LotEdge edge)
        {
            return ((1 << (int)edge) & BaseLotInfo.RoadsAlongEdges) != 0;
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