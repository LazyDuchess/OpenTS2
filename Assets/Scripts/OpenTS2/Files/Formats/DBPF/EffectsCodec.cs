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
            var life = Vector2Serializer.Deserialize(reader);
            var lifePreRoll = reader.ReadFloat();

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
            var rateSpeedScale = reader.ReadFloat();

            // Size.
            var sizeCurve = FloatCurve.Deserialize(reader);
            var sizeVary = reader.ReadFloat();

            // Aspect ratio?
            var aspectCurve = FloatCurve.Deserialize(reader);
            var aspectVary = reader.ReadFloat();

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

            // Alpha.
            var alphaCurve = FloatCurve.Deserialize(reader);
            var alphaVary = reader.ReadFloat();

            // Color.
            var colors = ReadMultipleVectors(reader);
            var colorVary = Vector3Serializer.Deserialize(reader);

            // Texture.
            var materialName = reader.ReadUint32PrefixedString();
            var tileCountU = reader.ReadByte();
            var tileCountV = reader.ReadByte();

            var particleAlignmentType = reader.ReadByte();
            var particleDrawType = reader.ReadByte();

            var layer = reader.ReadFloat();
            var frameSpeed = reader.ReadFloat();
            var frameStart = reader.ReadByte();
            var frameCount = reader.ReadByte();

            Vector3Serializer.Deserialize(reader);
            reader.ReadFloat();
            reader.ReadFloat();
            Vector3Serializer.Deserialize(reader);
            reader.ReadFloat();
            reader.ReadFloat();

            var screw = reader.ReadFloat();

            var wiggles = Wiggle.DeserializeList(reader);

            // Bloom.
            var bloomAlphaRate = reader.ReadByte();
            var bloomAlpha = reader.ReadByte();
            var bloomSizeRate = reader.ReadByte();
            var bloomSize = reader.ReadByte();

            var colliders = Collider.DeserializeList(reader);
            reader.ReadUint32PrefixedString();

            // Terrain interaction.
            var terrainBounce = reader.ReadFloat();
            var terrainRepelHeight = reader.ReadFloat();
            var terrainRepelStrength = reader.ReadFloat();
            var terrainRepelScout = reader.ReadFloat();
            var terrainRepelVertical = reader.ReadFloat();
            var terrainRepelKillHeight = reader.ReadFloat();
            var terrainDeathProbability = reader.ReadFloat();
            var deathByWaterProbability = reader.ReadFloat();

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

            return new ParticleEffect(flags: flags, life: life, lifePreRoll: lifePreRoll, rateDelay: rateDelay,
                rateTrigger: rateTrigger, emitDirection: emitDirectionBBox, emitSpeed: emitSpeed,
                emitVolume: emitVolumeBBox, emitTorusWidth: emitTorusWidth, rateCurve: rateCurve,
                rateCurveTime: rateCurveTime, rateCurveCycles: rateCurveCycles, rateSpeedScale: rateSpeedScale,
                sizeCurve: sizeCurve, sizeVary: sizeVary, aspectCurve: aspectCurve, aspectVary: aspectVary,
                rotateAxis: rotateAxis,
                rotateOffsetX: rotateOffsetX, rotateOffsetY: rotateOffsetY, rotateCurveX: rotateCurveX,
                rotateCurveY: rotateCurveY,
                alphaCurve: alphaCurve, alphaVary: alphaVary, colors: colors, colorVary: colorVary,
                materialName: materialName,
                tileCountU: tileCountU, tileCountV: tileCountV, particleAlignmentType: particleAlignmentType,
                particleDrawType: particleDrawType,
                layer: layer, frameSpeed: frameSpeed, frameStart: frameStart, frameCount: frameCount, screw: screw,
                wiggles: wiggles,
                bloomAlphaRate: bloomAlphaRate, bloomAlpha: bloomAlpha, bloomSizeRate: bloomSizeRate,
                bloomSize: bloomSize,
                colliders: colliders, terrainBounce: terrainBounce, terrainRepelHeight: terrainRepelHeight,
                terrainRepelStrength: terrainRepelStrength,
                terrainRepelScout: terrainRepelScout,
                terrainRepelVertical: terrainRepelVertical, terrainRepelKillHeight: terrainRepelKillHeight,
                terrainDeathProbability: terrainDeathProbability,
                deathByWaterProbability: deathByWaterProbability, randomWalkDelay: randomWalkDelay,
                randomWalkStrength: randomWalkStrength,
                randomWalkTurnX: randomWalkTurnX, randomWalkTurnY: randomWalkTurnY);
        }
    }
}