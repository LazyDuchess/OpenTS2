using UnityEngine;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Scenes.ParticleEffects;

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

        public GameObject CreateGameObject()
        {
            var gameObject = new GameObject(TextureName, typeof(SwarmDecal), typeof(MeshFilter), typeof(MeshRenderer));
            var swarmDecal = gameObject.GetComponent<SwarmDecal>();
            swarmDecal.SetDecal(this);
            return gameObject;
        }
    }
}