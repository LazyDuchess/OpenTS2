using OpenTS2.Files.Utils;
using UnityEngine;

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

        public AnimationCurve ToUnityCurve()
        {
            var keyframes = new Keyframe[Curve.Length];
            for (var i = 0; i < Curve.Length; i++)
            {
                keyframes[i] = new Keyframe(i, Curve[i]);
            }

            return new AnimationCurve(keyframes);
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