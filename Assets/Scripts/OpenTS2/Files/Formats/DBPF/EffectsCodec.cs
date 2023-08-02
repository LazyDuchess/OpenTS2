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
            for (var i = 0; i < metaParticles.Length; i++)
            {
                metaParticles[i] = ReadMetaParticleEffect(reader);
            }

            var decals = new DecalEffect[reader.ReadUInt32()];
            for (var i = 0; i < decals.Length; i++)
            {
                decals[i] = ReadDecalEffect(reader);
            }

            var numBrushes = reader.ReadUInt32();
            Debug.Assert(numBrushes == 0, "There shouldn't be any brush effects in Sims2");

            var numScrubbers = reader.ReadUInt32();
            Debug.Assert(numScrubbers == 0, "There shouldn't be any brush effects in Sims2");

            var sequences = new SequenceEffect[reader.ReadUInt32()];
            for (var i = 0; i < sequences.Length; i++)
            {
                sequences[i] = ReadSequenceEffect(reader);
            }

            var sounds = new SoundEffect[reader.ReadUInt32()];
            for (var i = 0; i < sounds.Length; i++)
            {
                sounds[i] = ReadSoundEffect(reader);
            }

            var cameras = new CameraEffect[reader.ReadUInt32()];

            return new EffectsAsset(particleEffects, metaParticles, decals, sequences, sounds, cameras);
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
            var life = ParticleHelper.ReadParticleLife(reader);

            // Rate and emission.
            var emission = ParticleHelper.ReadParticleEmission(reader, version, isMetaParticle:false);
            var rateSpeedScale = reader.ReadFloat();

            // Size and aspect.
            var size = ParticleHelper.ReadParticleSize(reader);

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
            var bloom = ParticleHelper.ReadParticleBloom(reader);
            var colliders = Collider.DeserializeList(reader);

            reader.ReadUint32PrefixedString();

            // Terrain interaction.
            var terrainInteraction = ParticleHelper.ReadParticleTerrainInteraction(reader);

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

        private static MetaParticle ReadMetaParticleEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version < 3);

            var flags = reader.ReadUInt64();
            var life = ParticleHelper.ReadParticleLife(reader);

            // Rate and emission.
            var emission = ParticleHelper.ReadParticleEmission(reader, version, isMetaParticle:true);

            // Size and aspect.
            var size = ParticleHelper.ReadParticleSize(reader);

            // Rotation.
            var rotateCurve = FloatCurve.Deserialize(reader);
            var rotateVary = reader.ReadFloat();
            var rotateOffset = reader.ReadFloat();

            // Color.
            var colors = ReadMultipleVectors(reader);
            var colorVary = Vector3Serializer.Deserialize(reader);
            var alphaCurve = FloatCurve.Deserialize(reader);
            var alphaVary = reader.ReadFloat();
            var color = new ParticleColor(alphaCurve, alphaVary, colors, colorVary);

            var baseEffect = reader.ReadUint32PrefixedString();
            var particleAlignmentType = reader.ReadByte();

            // A bunch of unknown stuff...
            Vector3Serializer.Deserialize(reader);
            reader.ReadFloat();
            reader.ReadFloat();
            Vector3Serializer.Deserialize(reader);
            reader.ReadFloat();

            var screw = reader.ReadFloat();
            var wiggles = Wiggle.DeserializeList(reader);
            var bloom = ParticleHelper.ReadParticleBloom(reader);
            var colliders = Collider.DeserializeList(reader);
            var terrainInteraction = ParticleHelper.ReadParticleTerrainInteraction(reader);

            Vector2Serializer.Deserialize(reader);

            var randomWalks = new RandomWalk[] { RandomWalk.Deserialize(reader), RandomWalk.Deserialize(reader) };
            var walkPreferDirection = Vector3Serializer.Deserialize(reader);

            var alignmentDamp = reader.ReadFloat();
            var alignmentBankWind = reader.ReadFloat();
            var alignmentBank = reader.ReadFloat();

            // These are guarded behind a `version < 4` guard but the game doesn't read meta-particles higher than 2.
            Vector3Serializer.Deserialize(reader);
            FloatCurve.Deserialize(reader);
            reader.ReadFloat();

            var tractorPoints = TractorPoint.DeserializeList(reader);
            reader.ReadFloat();

            return new MetaParticle(flags, life, emission, size, color, baseEffect);
        }

        private static DecalEffect ReadDecalEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version == 1);

            var flags = reader.ReadUInt32();

            var textureName = reader.ReadUint32PrefixedString();
            var decalDrawType = reader.ReadByte();

            var lifeType = reader.ReadByte();
            var life = reader.ReadFloat();

            var rotateCurve = FloatCurve.Deserialize(reader);
            var sizeCurve = FloatCurve.Deserialize(reader);
            var alphaCurve = FloatCurve.Deserialize(reader);
            var colors = ReadMultipleVectors(reader);
            var aspectCurve = FloatCurve.Deserialize(reader);
            var alphaVary = reader.ReadFloat();
            var sizeVary = reader.ReadFloat();
            var rotateVary = reader.ReadFloat();

            var textureRepeat = reader.ReadFloat();
            var textureOffset = Vector2Serializer.Deserialize(reader);

            return new DecalEffect(textureName, life, textureOffset);
        }

        private static SequenceEffect ReadSequenceEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version == 1);

            var components = new SequenceComponent[reader.ReadUInt32()];
            for (var i = 0; i < components.Length; i++)
            {
                components[i] = SequenceComponent.Deserialize(reader);
            }

            var flags = reader.ReadUInt32();
            return new SequenceEffect(components, flags);
        }

        private static SoundEffect ReadSoundEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version == 1);

            var flags = reader.ReadUInt32();
            var audioId = reader.ReadUInt32();
            var locationUpdateDelta = reader.ReadFloat();
            var playTime = reader.ReadFloat();
            var volume = reader.ReadFloat();

            return new SoundEffect(audioId, volume);
        }
    }
}