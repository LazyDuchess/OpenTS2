namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// Contains information about a lot in a neighborhood such as its type, road connections, position and elevation.
    /// </summary>
    public class LotInfoAsset : AbstractAsset
    {
        public int LotId { get; }
        public string LotName { get; }
        public string LotDescription { get; }
        public uint Width { get; }
        public uint Depth { get; }
        public uint LocationX { get; }
        public uint LocationY { get; }
        public float NeighborhoodToLotHeightOffset { get; }
        public byte FrontEdge { get; }

        public byte CreationFrontEdge { get; }

        public LotInfoAsset(int lotId, string lotName, string lotDescription,
            uint width, uint depth, uint locationX, uint locationY, float neighborhoodToLotHeightOffset,
            byte creationFrontEdge, byte frontEdge)
        {
            LotId = lotId;
            LotName = lotName;
            LotDescription = lotDescription;
            Width = width;
            Depth = depth;
            LocationX = locationX;
            LocationY = locationY;
            NeighborhoodToLotHeightOffset = neighborhoodToLotHeightOffset;
            CreationFrontEdge = creationFrontEdge;
            FrontEdge = frontEdge;
        }
    }
}