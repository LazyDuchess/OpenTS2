using UnityEngine;

namespace OpenTS2.Content.DBPF.Effects
{
    public readonly struct ModelEffect : IBaseEffect
    {
        public readonly string ModelName;
        public readonly float Size;
        public readonly Vector3 Color;
        public readonly float Alpha;

        public ModelEffect(string modelName, float size, Vector3 color, float alpha)
        {
            ModelName = modelName;
            Size = size;
            Color = color;
            Alpha = alpha;
        }
    }
}