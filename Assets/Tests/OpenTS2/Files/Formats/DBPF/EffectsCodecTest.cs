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
    }

    [Test]
    public void TestFirstParticleEffectIsCorrect()
    {
        var effectsAsset = ContentProvider.Get()
            .GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects, typeID: TypeIDs.EFFECTS));
        var particle = effectsAsset.Particles[0];

        Assert.That(particle.Life, Is.EqualTo(new Vector2(1, 1)));

        Assert.That(particle.MaterialName, Is.EqualTo("effects-puff"));
        Assert.That(particle.TileCountU, Is.EqualTo(1));
        Assert.That(particle.TileCountV, Is.EqualTo(1));

        Assert.That(particle.Wiggles.Length, Is.EqualTo(0));

        Assert.That(particle.Colliders.Length, Is.EqualTo(0));

        Assert.That(particle.TerrainBounce, Is.EqualTo(1.0f));
        Assert.That(particle.TerrainRepelKillHeight, Is.EqualTo(-1000000000.0f));
        Assert.That(particle.TerrainDeathProbability, Is.EqualTo(0.0f));
        Assert.That(particle.DeathByWaterProbability, Is.EqualTo(1.0f));

        Assert.That(particle.RandomWalkStrength, Is.EqualTo(new Vector2(50, 50)));
        Assert.That(particle.RandomWalkTurnX, Is.EqualTo(0.1).Within(0.005));
        Assert.That(particle.RandomWalkTurnY, Is.EqualTo(0.2).Within(0.005));
    }
}