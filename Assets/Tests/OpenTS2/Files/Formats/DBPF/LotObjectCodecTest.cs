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
        var lotObject = lotObjectAsset.Object;
        Assert.That(lotObject.ResourceName, Is.EqualTo("staircaseModularDeckRightRailing"));

        Assert.That(lotObject.Position.x, Is.EqualTo(35.3).Within(0.05));
        Assert.That(lotObject.Position.y, Is.EqualTo(20.5).Within(0.05));
        Assert.That(lotObject.Position.z, Is.EqualTo(0.2).Within(0.05));

        Assert.That(lotObject.Rotation.eulerAngles[0], Is.EqualTo(0));
        Assert.That(lotObject.Rotation.eulerAngles[1], Is.EqualTo(0));
        Assert.That(lotObject.Rotation.eulerAngles[2], Is.EqualTo(90.0));
    }

    [Test]
    public void TestSuccessfullyLoadsAnimatableObject()
    {
        var lotObjectAsset = ContentProvider.Get()
            .GetAsset<LotObjectAsset>(new ResourceKey(255, _groupID, TypeIDs.LOT_OBJECT));

        var lotObject = lotObjectAsset.Object;
        Assert.That(lotObject, Is.InstanceOf<LotObjectAsset.AnimatableObject>());
        Assert.That(lotObject.ResourceName, Is.EqualTo("flowerDaisy"));
    }
}