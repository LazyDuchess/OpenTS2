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

        public ParticleSystem.MinMaxCurve ToUnityCurve()
        {
            var keyframes = new Keyframe[Curve.Length];
            for (var i = 0; i < Curve.Length; i++)
            {
                keyframes[i] = new Keyframe(i, Curve[i]);
            }

            return new ParticleSystem.MinMaxCurve(1.0f, new AnimationCurve(keyframes));
        }

        public ParticleSystem.MinMaxCurve ToUnityCurveWithVariance(float vary)
        {
            var lowerKeyframes = new Keyframe[Curve.Length];
            var upperKeyframes = new Keyframe[Curve.Length];
            for (var i = 0; i < Curve.Length; i++)
            {
                var (minValue, maxValue) = FxVary.CalculateVary(Curve[i], vary);
                lowerKeyframes[i] = new Keyframe(i, minValue);
                upperKeyframes[i] = new Keyframe(i, maxValue);
            }

            return new ParticleSystem.MinMaxCurve(1.0f,
                new AnimationCurve(lowerKeyframes), new AnimationCurve(upperKeyframes));
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