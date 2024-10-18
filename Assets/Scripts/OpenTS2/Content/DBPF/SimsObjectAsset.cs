namespace OpenTS2.Content.DBPF
{
    public class SimsObjectAsset : AbstractAsset
    {
        public SimsObjectAsset(float tileLocationY, float tileLocationX, int level, float elevation, int objectGroupId,
            short[] attrs, short[] semiAttrs, short[] temp, short[] data, SimsObjectStackFrame[] stackFrames)
        {
            TileLocationY = tileLocationY;
            TileLocationX = tileLocationX;
            Level = level;
            Elevation = elevation;
            ObjectGroupId = objectGroupId;
            Attrs = attrs;
            SemiAttrs = semiAttrs;
            Temp = temp;
            Data = data;
            StackFrames = stackFrames;
        }

        public float TileLocationY { get; }
        public float TileLocationX { get; }
        public int Level { get; }
        public float Elevation { get; }
        public int ObjectGroupId { get; }

        public short[] Attrs { get; }
        public short[] SemiAttrs { get; }
        public short[] Temp { get; }
        public short[] Data { get; }

        public SimsObjectStackFrame[] StackFrames { get; }
    }

    public struct SimsObjectStackFrame
    {
        public ushort ObjectId;
        public ushort TreeId;
        public ushort BhavSaveType;

        public short[] Params;
        public short[] Locals;
    }
}