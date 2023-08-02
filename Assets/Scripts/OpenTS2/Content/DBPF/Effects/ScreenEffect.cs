using OpenTS2.Content.DBPF.Effects.Types;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Effects
{
    /// <summary>
    /// Applies a texture across the whole screen.
    /// </summary>
    public readonly struct ScreenEffect : IBaseEffect
    {
        public readonly Vector3[] Colors;
        public readonly FloatCurve Strength;
        public readonly float Length;
        public readonly float Delay;
        public readonly string Texture;

        public ScreenEffect(Vector3[] colors, FloatCurve strength, float length, float delay, string texture)
        {
            Colors = colors;
            Strength = strength;
            Length = length;
            Delay = delay;
            Texture = texture;
        }
    }
}