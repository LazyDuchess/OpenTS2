using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

public class EffectsCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Codecs/Effects.package");
    }

    [Test]
    public void TestSuccessfullyLoadsAndHasCorrectNumberOfEffects()
    {
        var effectsAsset = ContentProvider.Get()
            .GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects, typeID: TypeIDs.EFFECTS));

        Assert.IsNotNull(effectsAsset);
        Assert.That(effectsAsset.Particles.Length, Is.EqualTo(1792));
        Assert.That(effectsAsset.MetaParticles.Length, Is.EqualTo(302));
        Assert.That(effectsAsset.DecalEffects.Length, Is.EqualTo(23));
    }

    [Test]
    public void TestFirstParticleEffectIsCorrect()
    {
        var effectsAsset = ContentProvider.Get()
            .GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects, typeID: TypeIDs.EFFECTS));
        var particle = effectsAsset.Particles[0];

        Assert.That(particle.Life.Life, Is.EqualTo(new Vector2(1, 1)));

        Assert.That(particle.Drawing.MaterialName, Is.EqualTo("effects-puff"));
        Assert.That(particle.Drawing.TileCountU, Is.EqualTo(1));
        Assert.That(particle.Drawing.TileCountV, Is.EqualTo(1));

        Assert.That(particle.Wiggles.Length, Is.EqualTo(0));

        Assert.That(particle.Colliders.Length, Is.EqualTo(0));

        Assert.That(particle.TerrainInteraction.Bounce, Is.EqualTo(1.0f));
        Assert.That(particle.TerrainInteraction.KillHeight, Is.EqualTo(-1000000000.0f));
        Assert.That(particle.TerrainInteraction.TerrainDeathProbability, Is.EqualTo(0.0f));
        Assert.That(particle.TerrainInteraction.WaterDeathProbability, Is.EqualTo(1.0f));

        Assert.That(particle.RandomWalkStrength, Is.EqualTo(new Vector2(50, 50)));
        Assert.That(particle.RandomWalkTurnX, Is.EqualTo(0.1).Within(0.005));
        Assert.That(particle.RandomWalkTurnY, Is.EqualTo(0.2).Within(0.005));
    }

    [Test]
    public void TestFirstMetaParticleIsCorrect()
    {
        var effectsAsset = ContentProvider.Get()
            .GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects, typeID: TypeIDs.EFFECTS));
        var meta = effectsAsset.MetaParticles[0];

        Assert.That(meta.Life.Life.x, Is.EqualTo(0.1).Within(0.05));
        Assert.That(meta.Life.Life.y, Is.EqualTo(0.1).Within(0.05));
        Assert.That(meta.Life.LifePreRoll, Is.EqualTo(0.1).Within(0.05));

        Assert.That(meta.Emission.RateDelay, Is.EqualTo(new Vector2(-1, -1)));
        Assert.That(meta.Emission.EmitDirection.LowerCorner, Is.EqualTo(new Vector3(0, 0, 1)));
        Assert.That(meta.Emission.RateCurve.Curve, Is.EquivalentTo(new[] { 1.0f }));

        Assert.That(meta.Size.SizeCurve.Curve, Is.EquivalentTo(new[] { 1.0f }));
        Assert.That(meta.Size.SizeVary, Is.EqualTo(0));
        Assert.That(meta.Size.AspectCurve.Curve, Is.EquivalentTo(new[] { 1.0f }));
        Assert.That(meta.Size.AspectVary, Is.EqualTo(0));

        Assert.That(meta.BaseEffect, Is.EqualTo("construction_cursor_dust_effect"));

        Assert.That(meta.Color.AlphaCurve.Curve, Is.EquivalentTo(new[] { 1.0f }));
    }
}