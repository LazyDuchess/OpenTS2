using System;
using System.Collections.Generic;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Content.DBPF.Effects.Types;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;
using Collider = OpenTS2.Content.DBPF.Effects.Types.Collider;

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
            Debug.Assert(numScrubbers == 0, "There shouldn't be any scrubber effects in Sims2");

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
            for (var i = 0; i < cameras.Length; i++)
            {
                cameras[i] = ReadCameraEffect(reader);
            }

            var numGameEffects = reader.ReadUInt32();
            Debug.Assert(numGameEffects == 0, "There shouldn't be any game effects in Sims2");

            var models = new ModelEffect[reader.ReadUInt32()];
            for (var i = 0; i < models.Length; i++)
            {
                models[i] = ReadModelEffect(reader);
            }

            var screens = new ScreenEffect[reader.ReadUInt32()];
            for (var i = 0; i < screens.Length; i++)
            {
                screens[i] = ReadScreenEffect(reader);
            }

            var waters = new WaterEffect[reader.ReadUInt32()];
            for (var i = 0; i < waters.Length; i++)
            {
                waters[i] = ReadWaterEffect(reader);
            }

            var numLightEffects = reader.ReadUInt32();
            Debug.Assert(numLightEffects == 0, "There shouldn't be any light effects in Sims2");

            var visualEffectsVersion = reader.ReadUInt16();
            var visualEffects = new SwarmVisualEffect[reader.ReadUInt32()];
            for (var i = 0; i < visualEffects.Length; i++)
            {
                visualEffects[i] = ReadVisualEffect(reader, visualEffectsVersion);
            }

            var effectNamesToIds = new Dictionary<string, uint>();
            var effectName = "";
            do {
                effectName = reader.ReadUint32PrefixedString();
                var effectId = reader.ReadUInt32();
                effectNamesToIds[effectName] = effectId;
            } while (effectName != "end");

            return new EffectsAsset(particleEffects, metaParticles, decals, sequences, sounds, cameras, models, screens,
                waters, visualEffects, effectNamesToIds);
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
            var emission = ParticleHelper.ReadParticleEmission(reader, version, isMetaParticle: false);
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
            var emission = ParticleHelper.ReadParticleEmission(reader, version, isMetaParticle: true);

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

        private static CameraEffect ReadCameraEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version == 1);

            var flags = reader.ReadUInt32();
            var life = reader.ReadFloat();
            var shakeFadeLength = reader.ReadFloat();
            var shakeAmplitude = FloatCurve.Deserialize(reader);
            var shakeFrequency = FloatCurve.Deserialize(reader);
            var shakeAspect = reader.ReadFloat();
            var shakeType = reader.ReadByte();

            var heading = FloatCurve.Deserialize(reader);
            var pitch = FloatCurve.Deserialize(reader);
            var roll = FloatCurve.Deserialize(reader);
            var orbit = FloatCurve.Deserialize(reader);
            var fieldOfView = FloatCurve.Deserialize(reader);
            var nearClip = FloatCurve.Deserialize(reader);
            var farClip = FloatCurve.Deserialize(reader);

            var zoom = reader.ReadSByte();
            var rotate = reader.ReadSByte();
            var attachRadius = reader.ReadFloat();
            var cameraSelectName = reader.ReadUint32PrefixedString();

            return new CameraEffect(life, shakeAspect, cameraSelectName);
        }

        private static ModelEffect ReadModelEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version == 1);

            var modelName = reader.ReadUint32PrefixedString();
            var size = reader.ReadFloat();
            var color = Vector3Serializer.Deserialize(reader);
            var alpha = reader.ReadFloat();

            return new ModelEffect(modelName, size, color, alpha);
        }

        private static ScreenEffect ReadScreenEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version == 1);

            var mode = reader.ReadByte();
            var flags = reader.ReadUInt32();
            var colors = ReadMultipleVectors(reader);
            var strength = FloatCurve.Deserialize(reader);
            var length = reader.ReadFloat();
            var delay = reader.ReadFloat();
            var texture = reader.ReadUint32PrefixedString();

            return new ScreenEffect(colors, strength, length, delay, texture);
        }

        private static WaterEffect ReadWaterEffect(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version == 1);

            var flags = reader.ReadUInt32();
            if (flags == 0)
            {
                reader.ReadFloat();
                reader.ReadFloat();
            }
            else
            {
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadInt16();
                reader.ReadFloat();
                reader.ReadFloat();
                reader.ReadFloat();
                reader.ReadFloat();
            }

            reader.ReadFloat();

            return new WaterEffect(flags);
        }

        private static SwarmVisualEffect ReadVisualEffect(IoBuffer reader, ushort version)
        {
            var flags = reader.ReadUInt32();
            reader.ReadUInt16();
            if (version < 3)
            {
                reader.ReadUInt16();
            }
            else
            {
                reader.ReadUInt32();
            }

            reader.ReadUInt32();
            reader.ReadUInt32();
            Vector2Serializer.Deserialize(reader);

            var descriptions = new EffectDescription[reader.ReadUInt32()];
            for (var i = 0; i < descriptions.Length; i++)
            {
                descriptions[i] = EffectDescription.Deserialize(reader, version);
            }

            return new SwarmVisualEffect(descriptions);
        }
    }
}