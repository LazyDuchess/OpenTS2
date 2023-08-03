using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

public class EffectsCodecTest
{
    private EffectsAsset _effectsAsset;

    [OneTimeSetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Codecs/Effects.package");
        _effectsAsset = ContentProvider.Get()
            .GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects, typeID: TypeIDs.EFFECTS));
    }

    [Test]
    public void TestSuccessfullyLoadsAndHasCorrectNumberOfEffects()
    {
        Assert.IsNotNull(_effectsAsset);
        Assert.That(_effectsAsset.ParticleEffects.Length, Is.EqualTo(1792));
        Assert.That(_effectsAsset.MetaParticleEffects.Length, Is.EqualTo(302));
        Assert.That(_effectsAsset.DecalEffects.Length, Is.EqualTo(23));
        Assert.That(_effectsAsset.SequenceEffects.Length, Is.EqualTo(82));
        Assert.That(_effectsAsset.SoundEffects.Length, Is.EqualTo(109));
        Assert.That(_effectsAsset.CameraEffects.Length, Is.EqualTo(43));
        Assert.That(_effectsAsset.ModelEffects.Length, Is.EqualTo(40));
        Assert.That(_effectsAsset.WaterEffects.Length, Is.EqualTo(3));
        Assert.That(_effectsAsset.VisualEffects.Length, Is.EqualTo(1542));

        Assert.That(_effectsAsset.EffectNamesToIds["fireflamesfx"], Is.EqualTo(1276));
        Assert.That(_effectsAsset.EffectNamesToIds["washhandsinsink_adultviewer"], Is.EqualTo(1034));
    }

    [Test]
    public void TestFirstParticleEffectIsCorrect()
    {
        var particle = _effectsAsset.ParticleEffects[0];

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
        var meta = _effectsAsset.MetaParticleEffects[0];

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

    [Test]
    public void TestFirstDecalIsCorrect()
    {
        var decal = _effectsAsset.DecalEffects[0];

        Assert.That(decal.Life, Is.EqualTo(0.1).Within(0.05));
        Assert.That(decal.TextureName, Is.EqualTo("terrain_edit_ring"));
        Assert.That(decal.TextureOffset, Is.EqualTo(new Vector2(0, 0)));
    }

    [Test]
    public void TestFirstSequenceIsCorrect()
    {
        var sequence = _effectsAsset.SequenceEffects[0];

        Assert.That(sequence.Flags, Is.EqualTo(0x1));
        Assert.That(sequence.Components.Length, Is.EqualTo(1));

        var component = sequence.Components[0];
        Assert.That(component.ActivateTime, Is.EqualTo(new Vector2(-1, -1)));
        Assert.That(component.EffectName, Is.EqualTo("exptAfterImage"));
    }

    [Test]
    public void TestFirstSoundIsCorrect()
    {
        var sound = _effectsAsset.SoundEffects[0];

        Assert.That(sound.AudioId, Is.EqualTo(0xFF108046));
        Assert.That(sound.Volume, Is.EqualTo(0.0));
    }

    [Test]
    public void TestSixthCameraIsCorrect()
    {
        var camera = _effectsAsset.CameraEffects[6];

        Assert.That(camera.Life, Is.EqualTo(1.0));
        Assert.That(camera.ShakeAspect, Is.EqualTo(1.0));
        Assert.That(camera.CameraSelectName, Is.EqualTo("CAS_ZoomDisabled"));
    }

    [Test]
    public void TestFirstModelIsCorrect()
    {
        var model = _effectsAsset.ModelEffects[0];

        Assert.That(model.ModelName, Is.EqualTo("destruction_post_mouseup_cres"));
        Assert.That(model.Size, Is.EqualTo(1.0f));
        Assert.That(model.Color, Is.EqualTo(new Vector3(1, 1, 1)));
        Assert.That(model.Alpha, Is.EqualTo(1.0f));
    }

    [Test]
    public void TestThirdScreenIsCorrect()
    {
        var screen = _effectsAsset.ScreenEffects[3];

        Assert.That(screen.Strength.Curve, Is.EquivalentTo(new[] { 1.0f }));
        Assert.That(screen.Length, Is.EqualTo(0));
        Assert.That(screen.Delay, Is.EqualTo(0));
        Assert.That(screen.Texture, Is.EqualTo("letterbox"));
    }

    [Test]
    public void TestFirstWaterIsCorrect()
    {
        var water = _effectsAsset.WaterEffects[0];

        Assert.That(water.Flags, Is.EqualTo(0));
    }

    [Test]
    public void TestFirstVisualEffectIsCorrect()
    {
        var visualEffect = _effectsAsset.VisualEffects[0];
        Assert.That(visualEffect.Descriptions.Length, Is.EqualTo(1));

        var description = visualEffect.Descriptions[0];
        Assert.That(description.EffectName, Is.EqualTo("construction_cursor_dust"));
        Assert.That(description.EffectIndex, Is.EqualTo(0));
    }
}