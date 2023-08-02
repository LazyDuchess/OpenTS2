using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Effects
{
    public readonly struct MetaParticle : IBaseEffect
    {
        public readonly ulong Flags;

        public readonly ParticleLife Life;
        public readonly ParticleEmission Emission;

        public readonly ParticleSize Size;
        public readonly ParticleColor Color;

        public readonly string BaseEffect;

        public MetaParticle(ulong flags, ParticleLife life, ParticleEmission emission, ParticleSize size, ParticleColor color, string baseEffect)
        {
            Flags = flags;
            Life = life;
            Emission = emission;
            Size = size;
            Color = color;
            BaseEffect = baseEffect;
        }
    }

    public struct TractorPoint
    {
        public Vector3 UnknownVec1 { get; }
        public Vector3 UnknownVec2 { get; }
        public float UnknownFloat1 { get; }

        public TractorPoint(Vector3 unknownVec1, Vector3 unknownVec2, float unknownFloat1)
        {
            UnknownVec1 = unknownVec1;
            UnknownVec2 = unknownVec2;
            UnknownFloat1 = unknownFloat1;
        }

        public static TractorPoint Deserialize(IoBuffer reader)
        {
            return new TractorPoint(Vector3Serializer.Deserialize(reader), Vector3Serializer.Deserialize(reader),
                reader.ReadFloat());
        }

        public static TractorPoint[] DeserializeList(IoBuffer reader)
        {
            var tractorPoints = new TractorPoint[reader.ReadUInt32()];
            for (var i = 0; i < tractorPoints.Length; i++)
            {
                tractorPoints[i] = Deserialize(reader);
            }

            return tractorPoints;
        }
    }

    public struct RandomWalk
    {
        public Vector2 UnknownVec1 { get; }
        public Vector2 UnknownVec2 { get; }
        public float UnknownFloat1 { get; }
        public float UnknownFloat2 { get; }
        public float UnknownFloat3 { get; }

        public RandomWalk(Vector2 unknownVec1, Vector2 unknownVec2, float unknownFloat1, float unknownFloat2,
            float unknownFloat3)
        {
            UnknownVec1 = unknownVec1;
            UnknownVec2 = unknownVec2;
            UnknownFloat1 = unknownFloat1;
            UnknownFloat2 = unknownFloat2;
            UnknownFloat3 = unknownFloat3;
        }

        public static RandomWalk Deserialize(IoBuffer reader)
        {
            return new RandomWalk(Vector2Serializer.Deserialize(reader), Vector2Serializer.Deserialize(reader),
                reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        }
    }
}