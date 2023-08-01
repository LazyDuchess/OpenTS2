using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class EffectsCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Codecs/effects.package");
    }

    [Test]
    public void TestSuccessfullyLoadsAndHasCorrectNumberOfEffects()
    {
        var effectsAsset = ContentProvider.Get()
            .GetAsset<EffectsAsset>(new ResourceKey(instanceID: 1, groupID: GroupIDs.Effects, typeID: TypeIDs.EFFECTS));

        Assert.IsNotNull(effectsAsset);
        Assert.That(effectsAsset.Particles.Length, Is.EqualTo(1792));
    }
}