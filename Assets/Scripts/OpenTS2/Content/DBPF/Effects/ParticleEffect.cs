using UnityEngine;

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

        public readonly Vector2 Life;
        public readonly float LifePreRoll;

        public readonly Vector2 RateDelay;
        public readonly Vector2 RateTrigger;

        public readonly BoundingBox EmitDirection;
        public readonly Vector2 EmitSpeed;
        public readonly BoundingBox EmitVolume;
        public readonly float EmitTorusWidth;

        public readonly FloatCurve RateCurve;
        public readonly float RateCurveTime;
        public readonly ushort RateCurveCycles;
        public readonly float RateSpeedScale;

        public readonly FloatCurve SizeCurve;
        public readonly float SizeVary;

        public readonly FloatCurve AspectCurve;
        public readonly float AspectVary;

        public readonly Vector3 RotateAxis;
        public readonly float RotateOffsetX;
        public readonly float RotateOffsetY;
        public readonly FloatCurve RotateCurveX;
        public readonly FloatCurve RotateCurveY;

        public readonly FloatCurve AlphaCurve;
        public readonly float AlphaVary;

        public readonly Vector3[] Colors;
        public readonly Vector3 ColorVary;

        public readonly string MaterialName;
        public readonly byte TileCountU;
        public readonly byte TileCountV;

        public readonly byte ParticleAlignmentType;
        public readonly byte ParticleDrawType;

        public readonly float Layer;
        public readonly float FrameSpeed;
        public readonly byte FrameStart;
        public readonly byte FrameCount;

        public readonly float Screw;

        public readonly Wiggle[] Wiggles;

        public readonly byte BloomAlphaRate;
        public readonly byte BloomAlpha;
        public readonly byte BloomSizeRate;
        public readonly byte BloomSize;

        public readonly Collider[] Colliders;

        public readonly float TerrainBounce;
        public readonly float TerrainRepelHeight;
        public readonly float TerrainRepelStrength;
        public readonly float TerrainRepelScout;
        public readonly float TerrainRepelVertical;
        public readonly float TerrainRepelKillHeight;
        public readonly float TerrainDeathProbability;
        public readonly float DeathByWaterProbability;

        public readonly Vector2 RandomWalkDelay;
        public readonly Vector2 RandomWalkStrength;
        public readonly float RandomWalkTurnX;
        public readonly float RandomWalkTurnY;

        public ParticleEffect(ulong flags, Vector2 life, float lifePreRoll, Vector2 rateDelay, Vector2 rateTrigger,
            BoundingBox emitDirection, Vector2 emitSpeed, BoundingBox emitVolume, float emitTorusWidth,
            FloatCurve rateCurve, float rateCurveTime, ushort rateCurveCycles, float rateSpeedScale,
            FloatCurve sizeCurve, float sizeVary, FloatCurve aspectCurve, float aspectVary, Vector3 rotateAxis,
            float rotateOffsetX, float rotateOffsetY, FloatCurve rotateCurveX, FloatCurve rotateCurveY,
            FloatCurve alphaCurve, float alphaVary, Vector3[] colors, Vector3 colorVary, string materialName,
            byte tileCountU, byte tileCountV, byte particleAlignmentType, byte particleDrawType, float layer,
            float frameSpeed, byte frameStart, byte frameCount, float screw, Wiggle[] wiggles, byte bloomAlphaRate,
            byte bloomAlpha, byte bloomSizeRate, byte bloomSize, Collider[] colliders, float terrainBounce,
            float terrainRepelHeight, float terrainRepelStrength, float terrainRepelScout, float terrainRepelVertical, float terrainRepelKillHeight,
            float terrainDeathProbability, float deathByWaterProbability, Vector2 randomWalkDelay,
            Vector2 randomWalkStrength, float randomWalkTurnX, float randomWalkTurnY)
        {
            Flags = flags;
            Life = life;
            LifePreRoll = lifePreRoll;
            RateDelay = rateDelay;
            RateTrigger = rateTrigger;
            EmitDirection = emitDirection;
            EmitSpeed = emitSpeed;
            EmitVolume = emitVolume;
            EmitTorusWidth = emitTorusWidth;
            RateCurve = rateCurve;
            RateCurveTime = rateCurveTime;
            RateCurveCycles = rateCurveCycles;
            RateSpeedScale = rateSpeedScale;
            SizeCurve = sizeCurve;
            SizeVary = sizeVary;
            AspectCurve = aspectCurve;
            AspectVary = aspectVary;
            RotateAxis = rotateAxis;
            RotateOffsetX = rotateOffsetX;
            RotateOffsetY = rotateOffsetY;
            RotateCurveX = rotateCurveX;
            RotateCurveY = rotateCurveY;
            AlphaCurve = alphaCurve;
            AlphaVary = alphaVary;
            Colors = colors;
            ColorVary = colorVary;
            MaterialName = materialName;
            TileCountU = tileCountU;
            TileCountV = tileCountV;
            ParticleAlignmentType = particleAlignmentType;
            ParticleDrawType = particleDrawType;
            Layer = layer;
            FrameSpeed = frameSpeed;
            FrameStart = frameStart;
            FrameCount = frameCount;
            Screw = screw;
            Wiggles = wiggles;
            BloomAlphaRate = bloomAlphaRate;
            BloomAlpha = bloomAlpha;
            BloomSizeRate = bloomSizeRate;
            BloomSize = bloomSize;
            Colliders = colliders;
            TerrainBounce = terrainBounce;
            TerrainRepelHeight = terrainRepelHeight;
            TerrainRepelStrength = terrainRepelStrength;
            TerrainRepelScout = terrainRepelScout;
            TerrainRepelVertical = terrainRepelVertical;
            TerrainRepelKillHeight = terrainRepelKillHeight;
            TerrainDeathProbability = terrainDeathProbability;
            DeathByWaterProbability = deathByWaterProbability;
            RandomWalkDelay = randomWalkDelay;
            RandomWalkStrength = randomWalkStrength;
            RandomWalkTurnX = randomWalkTurnX;
            RandomWalkTurnY = randomWalkTurnY;
        }
    }
}