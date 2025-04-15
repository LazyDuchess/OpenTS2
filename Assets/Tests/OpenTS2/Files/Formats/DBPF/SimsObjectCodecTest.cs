using System.Linq;
using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class SimsObjectCodecTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        _groupID = ContentManager.Instance.AddPackage("TestAssets/Codecs/ObjCodecs.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsSimsObject()
    {
        // Pancake object.
        var objectAsset = ContentManager.Instance.GetAsset<SimsObjectAsset>(
            new ResourceKey(163, _groupID, TypeIDs.XOBJ));

        Assert.That(objectAsset, Is.Not.Null);
    }
}