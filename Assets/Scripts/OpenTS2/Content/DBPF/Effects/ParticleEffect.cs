using System;
using OpenTS2.Content.DBPF.Effects.Types;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;
using Collider = OpenTS2.Content.DBPF.Effects.Types.Collider;

namespace OpenTS2.Content.DBPF.Effects
{
    /// <summary>
    /// This giant structure is how the game stores effects. Ideally we should only do this for now
    /// until we can split off things like "size" and "rotation" with all their different fields
    /// into different sub-types.
    /// </summary>
    public readonly struct ParticleEffect : IBaseEffect
    {
        public readonly ulong Flags;

        public readonly ParticleLife Life;

        public readonly ParticleEmission Emission;
        public readonly float RateSpeedScale;

        public readonly ParticleSize Size;

        public readonly Vector3 RotateAxis;
        public readonly float RotateOffsetX;
        public readonly float RotateOffsetY;
        public readonly FloatCurve RotateCurveX;
        public readonly FloatCurve RotateCurveY;

        public readonly ParticleColor Color;
        public readonly ParticleDrawing Drawing;

        public readonly float Screw;

        public readonly Wiggle[] Wiggles;

        public readonly ParticleBloom Bloom;

        public readonly Collider[] Colliders;

        public readonly ParticleTerrainInteraction TerrainInteraction;

        public readonly Vector2 RandomWalkDelay;
        public readonly Vector2 RandomWalkStrength;
        public readonly float RandomWalkTurnX;
        public readonly float RandomWalkTurnY;

        public ParticleEffect(ulong flags, ParticleLife life, ParticleEmission emission, float rateSpeedScale,
            ParticleSize size, Vector3 rotateAxis,
            float rotateOffsetX, float rotateOffsetY, FloatCurve rotateCurveX, FloatCurve rotateCurveY,
            ParticleColor color, ParticleDrawing drawing, float screw, Wiggle[] wiggles, ParticleBloom bloom,
            Collider[] colliders,
            ParticleTerrainInteraction terrainInteraction, Vector2 randomWalkDelay,
            Vector2 randomWalkStrength, float randomWalkTurnX, float randomWalkTurnY)
        {
            Flags = flags;
            Life = life;
            Emission = emission;
            RateSpeedScale = rateSpeedScale;
            Size = size;
            RotateAxis = rotateAxis;
            RotateOffsetX = rotateOffsetX;
            RotateOffsetY = rotateOffsetY;
            RotateCurveX = rotateCurveX;
            RotateCurveY = rotateCurveY;
            Color = color;
            Drawing = drawing;
            Screw = screw;
            Wiggles = wiggles;
            Bloom = bloom;
            Colliders = colliders;
            TerrainInteraction = terrainInteraction;
            RandomWalkDelay = randomWalkDelay;
            RandomWalkStrength = randomWalkStrength;
            RandomWalkTurnX = randomWalkTurnX;
            RandomWalkTurnY = randomWalkTurnY;
        }

        public bool IsFlagSet(ParticleFlagBits flag)
        {
            return flag.IsFlagSet(Flags);
        }
    }

    public enum ParticleFlagBits
    {
        /// <summary>
        /// This flag is set if the particle emitter should be an ellipsoid shape.
        /// </summary>
        EmitterIsEllipsoid = 8,
        ParticleHasMaterial = 21,
        ParticleHasShape = 22,
        ParticleMaterialIsLight = 23,
    }

    /// <summary>
    /// Extension method so `IsFlagSet` can be called on a plain long in shared code between Particle and MetaParticle.
    /// </summary>
    public static class ParticleFlagBitsExtensions {
        public static bool IsFlagSet(this ParticleFlagBits flagBit, ulong flags)
        {
            var mask = 1UL << (int)flagBit;
            return (flags & mask) != 0;
        }
    }

    public struct ParticleLife
    {
        /// <summary>
        /// Particles get a lifetime in a random range between this vector's start and end.
        /// </summary>
        public Vector2 Life { get; }

        public float LifePreRoll { get; }

        public ParticleLife(Vector2 life, float lifePreRoll)
        {
            Life = life;
            LifePreRoll = lifePreRoll;
        }
    }

    public struct ParticleEmission
    {
        public Vector2 RateDelay { get; }
        public Vector2 RateTrigger { get; }

        public BoundingBox EmitDirection { get; }
        public Vector2 EmitSpeed { get; }

        /// <summary>
        /// The volume of the emitter defined by its corners. This can either be a cuboid shape, an ellipsoid or a
        /// torus depending on the flags and EmitTorusWidth.
        /// </summary>
        public BoundingBox EmitVolume { get; }

        /// <summary>
        /// If set to a value greater than 0, emitter is a torus shape.
        /// </summary>
        public float EmitTorusWidth { get; }

        public FloatCurve RateCurve { get; }
        public float RateCurveTime { get; }
        public uint RateCurveCycles { get; }

        public ParticleEmission(Vector2 rateDelay, Vector2 rateTrigger, BoundingBox emitDirection, Vector2 emitSpeed,
            BoundingBox emitVolume, float emitTorusWidth, FloatCurve rateCurve, float rateCurveTime,
            uint rateCurveCycles)
        {
            RateDelay = rateDelay;
            RateTrigger = rateTrigger;
            EmitDirection = emitDirection;
            EmitSpeed = emitSpeed;
            EmitVolume = emitVolume;
            EmitTorusWidth = emitTorusWidth;
            RateCurve = rateCurve;
            RateCurveTime = rateCurveTime;
            RateCurveCycles = rateCurveCycles;
        }
    }

    public struct ParticleSize
    {
        public FloatCurve SizeCurve { get; }
        public float SizeVary { get; }

        public FloatCurve AspectCurve { get; }
        public float AspectVary { get; }

        public ParticleSize(FloatCurve sizeCurve, float sizeVary, FloatCurve aspectCurve, float aspectVary)
        {
            SizeCurve = sizeCurve;
            SizeVary = sizeVary;
            AspectCurve = aspectCurve;
            AspectVary = aspectVary;
        }
    }

    public struct ParticleColor
    {
        public FloatCurve AlphaCurve { get; }
        public float AlphaVary { get; }

        public Vector3[] Colors { get; }
        public Vector3 ColorVary { get; }

        public ParticleColor(FloatCurve alphaCurve, float alphaVary, Vector3[] colors, Vector3 colorVary)
        {
            AlphaCurve = alphaCurve;
            AlphaVary = alphaVary;
            Colors = colors;
            ColorVary = colorVary;
        }

        private readonly (Color, Color) GetMinMaxColorAtTimeStep(int time)
        {
            var min = new Color(FxVary.VaryValueMin(Colors[time].x, ColorVary[0]),
                FxVary.VaryValueMin(Colors[time].y, ColorVary[1]),
                FxVary.VaryValueMin(Colors[time].z, ColorVary[2]));
            var max = new Color(FxVary.VaryValueMax(Colors[time].x, ColorVary[0]),
                FxVary.VaryValueMax(Colors[time].y, ColorVary[1]),
                FxVary.VaryValueMax(Colors[time].z, ColorVary[2]));
            if (time < AlphaCurve.Curve.Length)
            {
                var (minAlpha, maxAlpha) = GetMinMaxAlphaAtTimeStep(time);
                min.a = minAlpha;
                max.a = maxAlpha;
            }
            return (min, max);
        }

        private readonly (float, float) GetMinMaxAlphaAtTimeStep(int time)
        {
            var min = Math.Max(0, FxVary.VaryValueMin(AlphaCurve.Curve[time], AlphaVary));
            var max = Math.Min(1, FxVary.VaryValueMax(AlphaCurve.Curve[time], AlphaVary));
            return (min, max);
        }

        /// <summary>
        /// This returns the range of colors to use for start of the particle.
        /// </summary>
        public readonly (Color, Color) GetStartingColorRange()
        {
            return GetMinMaxColorAtTimeStep(0);
        }

        public readonly (Gradient, Gradient) GetColorGradientsOverTime()
        {
            // Unfortunately, unity supports only a maximum of 8 keys so we have to pick the nearest time step from the
            // swarm system for now.
            if (Colors.Length >= 8 || AlphaCurve.Curve.Length >= 8)
            {
                Debug.LogWarning("Effect has too many color or alpha values");
            }
            var colorLength = Math.Min(Colors.Length, 8);
            var alphaLength = Math.Min(AlphaCurve.Curve.Length, 8);

            var minColorKeys = new GradientColorKey[colorLength];
            var maxColorKeys = new GradientColorKey[colorLength];

            for (var i = 0; i < colorLength; i++)
            {
                var time = ((float)i) / Colors.Length;
                var colorIndex = (int) Math.Floor((float)i / colorLength * Colors.Length);

                var (min, max) = GetMinMaxColorAtTimeStep(colorIndex);
                minColorKeys[i] = new GradientColorKey(min, time);
                maxColorKeys[i] = new GradientColorKey(max, time);
            }

            var minAlphaKeys = new GradientAlphaKey[alphaLength];
            var maxAlphaKeys = new GradientAlphaKey[alphaLength];
            for (var i = 0; i < alphaLength; i++)
            {
                var time = ((float)i) / AlphaCurve.Curve.Length;
                var alphaIndex = (int) Math.Floor((float)i / alphaLength * AlphaCurve.Curve.Length);

                var (min, max) = GetMinMaxAlphaAtTimeStep(alphaIndex);
                minAlphaKeys[i] = new GradientAlphaKey(min, time);
                maxAlphaKeys[i] = new GradientAlphaKey(max, time);
            }

            var minGradient = new Gradient();
            minGradient.SetKeys(minColorKeys, minAlphaKeys);
            var maxGradient = new Gradient();
            maxGradient.SetKeys(maxColorKeys, maxAlphaKeys);
            return (minGradient, maxGradient);
        }
    }

    public struct ParticleDrawing
    {
        public string MaterialName { get; }
        public byte TileCountU { get; }
        public byte TileCountV { get; }

        public byte ParticleAlignmentType { get; }
        public DrawType ParticleDrawType { get; }

        public float Layer { get; }
        public float FrameSpeed { get; }
        public byte FrameStart { get; }
        public byte FrameCount { get; }

        public ParticleDrawing(string materialName, byte tileCountU, byte tileCountV, byte particleAlignmentType,
            byte particleDrawType, float layer, float frameSpeed, byte frameStart, byte frameCount)
        {
            MaterialName = materialName;
            TileCountU = tileCountU;
            TileCountV = tileCountV;
            ParticleAlignmentType = particleAlignmentType;
            if (!Enum.IsDefined(typeof(DrawType), particleDrawType))
            {
                throw new ArgumentException($"Invalid particleDrawType: {particleDrawType}");
            }
            ParticleDrawType = (DrawType)particleDrawType;
            Layer = layer;
            FrameSpeed = frameSpeed;
            FrameStart = frameStart;
            FrameCount = frameCount;
        }

        public enum DrawType : byte
        {
            Decal = 0,
            DecalInvertDepth = 1,
            DecalIgnoreDepth = 2,
            DepthDecal = 3,
            Additive = 4,
            AdditiveInvertDepth = 5,
            AdditiveIgnoreDepth = 6,
            Modulate = 7,
        }
    }

    public struct ParticleTerrainInteraction
    {
        public float Bounce { get; }
        public float RepelHeight { get; }
        public float RepelStrength { get; }
        public float RepelScout { get; }
        public float RepelVertical { get; }
        public float KillHeight { get; }
        public float TerrainDeathProbability { get; }
        public float WaterDeathProbability { get; }

        public ParticleTerrainInteraction(float bounce, float repelHeight, float repelStrength, float repelScout,
            float repelVertical, float killHeight, float terrainDeathProbability, float waterDeathProbability)
        {
            Bounce = bounce;
            RepelHeight = repelHeight;
            RepelStrength = repelStrength;
            RepelScout = repelScout;
            RepelVertical = repelVertical;
            KillHeight = killHeight;
            TerrainDeathProbability = terrainDeathProbability;
            WaterDeathProbability = waterDeathProbability;
        }
    }

    public struct ParticleBloom
    {
        public byte AlphaRate { get; }
        public byte Alpha { get; }
        public byte SizeRate { get; }
        public byte Size { get; }

        public ParticleBloom(byte alphaRate, byte alpha, byte sizeRate, byte size)
        {
            AlphaRate = alphaRate;
            Alpha = alpha;
            SizeRate = sizeRate;
            Size = size;
        }
    }

    // These are methods common to both Particle and MetaParticles.

    #region Particle and MetaParticle common readers

    public static class ParticleHelper
    {
        internal static ParticleLife ReadParticleLife(IoBuffer reader)
        {
            return new ParticleLife(life: Vector2Serializer.Deserialize(reader), lifePreRoll: reader.ReadFloat());
        }

        internal static ParticleEmission ReadParticleEmission(IoBuffer reader, ushort version, bool isMetaParticle)
        {
            // Rate and emission.
            var rateDelay = Vector2Serializer.Deserialize(reader);
            var rateTrigger = Vector2Serializer.Deserialize(reader);

            var emitDirectionBBox = BoundingBox.Deserialize(reader);
            var emitSpeed = Vector2Serializer.Deserialize(reader);
            var emitVolumeBBox = BoundingBox.Deserialize(reader);
            var emitTorusWidth = -1.0f;
            if ((isMetaParticle && version > 1) || (!isMetaParticle && version > 2))
            {
                emitTorusWidth = reader.ReadFloat();
            }

            var rateCurve = FloatCurve.Deserialize(reader);
            var rateCurveTime = reader.ReadFloat();
            // Yup, for some reason even though it's the same field, particles read as a short whereas metaparticles
            // read it as an int...
            var rateCurveCycles = isMetaParticle switch
            {
                true => reader.ReadUInt32(),
                false => reader.ReadUInt16(),
            };

            return new ParticleEmission(rateDelay, rateTrigger, emitDirectionBBox, emitSpeed, emitVolumeBBox,
                emitTorusWidth, rateCurve, rateCurveTime, rateCurveCycles);
        }

        internal static ParticleSize ReadParticleSize(IoBuffer reader)
        {
            return new ParticleSize(
                sizeCurve: FloatCurve.Deserialize(reader), sizeVary: reader.ReadFloat(),
                aspectCurve: FloatCurve.Deserialize(reader), aspectVary: reader.ReadFloat());
        }

        internal static ParticleBloom ReadParticleBloom(IoBuffer reader)
        {
            return new ParticleBloom(alphaRate: reader.ReadByte(), alpha: reader.ReadByte(),
                sizeRate: reader.ReadByte(), size: reader.ReadByte());
        }

        internal static ParticleTerrainInteraction ReadParticleTerrainInteraction(IoBuffer reader)
        {
            return new ParticleTerrainInteraction(
                bounce: reader.ReadFloat(),
                repelHeight: reader.ReadFloat(),
                repelStrength: reader.ReadFloat(),
                repelScout: reader.ReadFloat(),
                repelVertical: reader.ReadFloat(),
                killHeight: reader.ReadFloat(),
                terrainDeathProbability: reader.ReadFloat(), waterDeathProbability: reader.ReadFloat()
            );
        }
    }

    #endregion
}