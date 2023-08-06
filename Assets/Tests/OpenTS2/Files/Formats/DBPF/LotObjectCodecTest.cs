using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

public class LotObjectCodecTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        _groupID = ContentProvider.Get().AddPackage("TestAssets/Codecs/LotObject.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsLotObject()
    {
        var lotObjectAsset = ContentProvider.Get()
            .GetAsset<LotObjectAsset>(new ResourceKey(199, _groupID, TypeIDs.LOT_OBJECT));

        Assert.IsNotNull(lotObjectAsset);
        Assert.That(lotObjectAsset.ResourceName, Is.EqualTo("staircaseModularDeckRightRailing"));

        Assert.That(lotObjectAsset.Position.x, Is.EqualTo(35.3).Within(0.05));
        Assert.That(lotObjectAsset.Position.y, Is.EqualTo(20.5).Within(0.05));
        Assert.That(lotObjectAsset.Position.z, Is.EqualTo(0.2).Within(0.05));

        Assert.That(lotObjectAsset.Rotation.eulerAngles[0], Is.EqualTo(0));
        Assert.That(lotObjectAsset.Rotation.eulerAngles[1], Is.EqualTo(0));
        Assert.That(lotObjectAsset.Rotation.eulerAngles[2], Is.EqualTo(90.0));
    }
}