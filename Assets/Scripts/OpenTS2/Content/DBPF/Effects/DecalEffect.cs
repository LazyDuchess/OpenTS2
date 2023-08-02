using UnityEngine;

namespace OpenTS2.Content.DBPF.Effects
{
    public readonly struct DecalEffect : IBaseEffect
    {
        public readonly string TextureName;
        public readonly float Life;
        public readonly Vector2 TextureOffset;

        public DecalEffect(string textureName, float life, Vector2 textureOffset)
        {
            TextureName = textureName;
            Life = life;
            TextureOffset = textureOffset;
        }
    }
}