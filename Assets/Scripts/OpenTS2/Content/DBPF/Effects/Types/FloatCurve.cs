using OpenTS2.Files.Utils;

namespace OpenTS2.Content.DBPF.Effects.Types
{
    /// <summary>
    /// A "curve" of changing float values.
    /// </summary>
    public readonly struct FloatCurve
    {
        public readonly float[] Curve;

        public FloatCurve(float[] curve)
        {
            Curve = curve;
        }

        public static FloatCurve Deserialize(IoBuffer reader)
        {
            var curve = new float[reader.ReadUInt32()];
            for (var i = 0; i < curve.Length; i++)
            {
                curve[i] = reader.ReadFloat();
            }
            return new FloatCurve(curve);
        }
    }
}