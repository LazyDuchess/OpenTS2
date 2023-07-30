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
        public uint LocationX { get; }
        public uint LocationY { get; }
        public float NeighborhoodToLotHeightOffset { get; }
        public byte FrontEdge { get; }

        public LotInfoAsset(int lotId, string lotName, string lotDescription, uint locationX, uint locationY,
            float neighborhoodToLotHeightOffset, byte frontEdge) =>
            (LotId, LotName, LotDescription, LocationX, LocationY, NeighborhoodToLotHeightOffset, FrontEdge) = (lotId,
                lotName,
                lotDescription,
                locationX, locationY, neighborhoodToLotHeightOffset, frontEdge);
    }
}