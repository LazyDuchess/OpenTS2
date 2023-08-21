using OpenTS2.Scenes.Lot;

namespace OpenTS2.Content.DBPF
{
    public struct FencePost
    {
        public int Level;
        public float XPos;
        public float YPos;
        public uint GUID;

        public override string ToString()
        {
            return $"({XPos}, {YPos}, {Level}): {GUID:x8}";
        }
    }

    public class FencePostLayerAsset : AbstractAsset
    {
        public FencePost[] Entries { get; }

        public FencePostLayerAsset(FencePost[] entries)
        {
            Entries = entries;
        }
    }

}