using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class SimAppearanceCodecTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        _groupID = ContentManager.Instance.AddPackage("TestAssets/Codecs/SimData.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsAppearanceObject()
    {
        var simAppearanceAsset = ContentManager.Instance
            .GetAsset<SimAppearanceAsset>(new ResourceKey(1, _groupID, TypeIDs.SIM_APPEARANCE));

        Assert.IsNotNull(simAppearanceAsset);
        Assert.AreEqual(simAppearanceAsset.Stretch, 0.94, delta:.01);
    }
}