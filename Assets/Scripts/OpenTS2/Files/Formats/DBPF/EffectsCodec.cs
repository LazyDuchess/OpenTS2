using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;
using Collider = OpenTS2.Content.DBPF.Effects.Collider;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.EFFECTS)]
    public class EffectsCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var version = reader.ReadUInt16();
            Debug.Assert(version == 0);

            var particleEffects = new ParticleEffect[reader.ReadUInt32()];
            for (var i = 0; i < particleEffects.Length; i++)
            {
                particleEffects[i] = ReadParticleEffect(reader);
            }

            var metaParticles = new MetaParticle[reader.ReadUInt32()];

            return new EffectsAsset(particleEffects, metaParticles);
        }

        private static Vector3[] ReadMultipleVectors(IoBuffer reader)
        {
            var vectors = new Vector3[reader.ReadUInt32()];
            for (var i = 0; i < vectors.Length; i++)
            {
                vectors[i] = Vector3Serializer.Deserialize(reader);
            }

            return vectors;
        }

        private static ParticleEffect ReadParticleEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version < 10);

            var flags = version < 5 ? reader.ReadUInt32() : reader.ReadUInt64();

            // Particle life.
            var life = new ParticleLife(life: Vector2Serializer.Deserialize(reader), lifePreRoll: reader.ReadFloat());

            // Rate and emission.
            var rateDelay = Vector2Serializer.Deserialize(reader);
            var rateTrigger = Vector2Serializer.Deserialize(reader);

            var emitDirectionBBox = BoundingBox.Deserialize(reader);
            var emitSpeed = Vector2Serializer.Deserialize(reader);
            var emitVolumeBBox = BoundingBox.Deserialize(reader);
            var emitTorusWidth = -1.0f;
            if (version > 2)
            {
                emitTorusWidth = reader.ReadFloat();
            }

            var rateCurve = FloatCurve.Deserialize(reader);
            var rateCurveTime = reader.ReadFloat();
            var rateCurveCycles = reader.ReadUInt16();

            var emission = new ParticleEmission(rateDelay, rateTrigger, emitDirectionBBox, emitSpeed, emitVolumeBBox,
                emitTorusWidth, rateCurve, rateCurveTime, rateCurveCycles);
            var rateSpeedScale = reader.ReadFloat();

            var size = new ParticleSize(
                sizeCurve: FloatCurve.Deserialize(reader), sizeVary: reader.ReadFloat(),
                aspectCurve: FloatCurve.Deserialize(reader), aspectVary: reader.ReadFloat());

            // Rotation stuff, a bunch of unknowns here.
            var rotateAxis = Vector3Serializer.Deserialize(reader);
            Vector3Serializer.Deserialize(reader);
            reader.ReadByte();
            if (version < 6)
            {
                reader.ReadFloat();
                reader.ReadFloat();
            }
            else
            {
                reader.ReadFloat();
                reader.ReadFloat();
                reader.ReadFloat();
                reader.ReadFloat();
            }

            var rotateOffsetX = reader.ReadFloat();
            var rotateOffsetY = reader.ReadFloat();
            var rotateCurveX = FloatCurve.Deserialize(reader);
            var rotateCurveY = FloatCurve.Deserialize(reader);
            if (version > 4)
            {
                ReadMultipleVectors(reader);
                Vector3Serializer.Deserialize(reader);
            }

            // Color.
            var alphaCurve = FloatCurve.Deserialize(reader);
            var alphaVary = reader.ReadFloat();
            var colors = ReadMultipleVectors(reader);
            var colorVary = Vector3Serializer.Deserialize(reader);
            var color = new ParticleColor(alphaCurve, alphaVary, colors, colorVary);

            // Texture.
            var drawing = new ParticleDrawing(
                materialName: reader.ReadUint32PrefixedString(),
                tileCountU: reader.ReadByte(), tileCountV: reader.ReadByte(),
                particleAlignmentType: reader.ReadByte(), particleDrawType: reader.ReadByte(),
                layer: reader.ReadFloat(), frameSpeed: reader.ReadFloat(), frameStart: reader.ReadByte(),
                frameCount: reader.ReadByte()
            );

            Vector3Serializer.Deserialize(reader);
            reader.ReadFloat();
            reader.ReadFloat();
            Vector3Serializer.Deserialize(reader);
            reader.ReadFloat();
            reader.ReadFloat();

            var screw = reader.ReadFloat();

            var wiggles = Wiggle.DeserializeList(reader);

            // Bloom.
            var bloom = new ParticleBloom(alphaRate: reader.ReadByte(), alpha: reader.ReadByte(),
                sizeRate: reader.ReadByte(), size: reader.ReadByte());

            var colliders = Collider.DeserializeList(reader);
            reader.ReadUint32PrefixedString();

            // Terrain interaction.
            var terrainInteraction = new ParticleTerrainInteraction(
                bounce: reader.ReadFloat(),
                repelHeight: reader.ReadFloat(),
                repelStrength: reader.ReadFloat(),
                repelScout: reader.ReadFloat(),
                repelVertical: reader.ReadFloat(),
                killHeight: reader.ReadFloat(),
                terrainDeathProbability: reader.ReadFloat(), waterDeathProbability: reader.ReadFloat()
            );

            Vector2Serializer.Deserialize(reader);

            var randomWalkDelay = Vector2Serializer.Deserialize(reader);
            var randomWalkStrength = Vector2Serializer.Deserialize(reader);
            var randomWalkTurnX = reader.ReadFloat();
            var randomWalkTurnY = reader.ReadFloat();

            if (version < 8)
            {
                // Something to do with attractors.
                Vector3Serializer.Deserialize(reader);
                var attractorStrength = FloatCurve.Deserialize(reader);
                reader.ReadFloat();
            }

            if (version >= 5)
            {
                throw new NotImplementedException("Particle versions 5 and above not implemented yet");
            }

            return new ParticleEffect(flags, life, emission, rateSpeedScale: rateSpeedScale,
                size, rotateAxis, rotateOffsetX, rotateOffsetY: rotateOffsetY, rotateCurveX: rotateCurveX, rotateCurveY,
                color, drawing, screw, wiggles, bloom,
                colliders, terrainInteraction, randomWalkDelay, randomWalkStrength, randomWalkTurnX, randomWalkTurnY);
        }
    }
}