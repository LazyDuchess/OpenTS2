namespace OpenTS2.Content.DBPF
{
    public struct PoolEntry
    {
        public int Id;
        public float XPos;
        public float YPos;
        public int Level;
        public int XSize;
        public int YSize;
        public int Unknown2;
        public float YOffset;
        public int Unknown3;
    }

    public class PoolAsset : AbstractAsset
    {
        public PoolEntry[] Entries { get; }

        public PoolAsset(PoolEntry[] entries)
        {
            Entries = entries;
        }
    }

}