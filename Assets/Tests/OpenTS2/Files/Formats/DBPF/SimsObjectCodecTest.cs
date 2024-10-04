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
        TestMain.Initialize();
        _groupID = ContentProvider.Get().AddPackage("TestAssets/Codecs/ObjCodecs.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsSimsObject()
    {
        var objectAsset = ContentProvider.Get().GetAsset<SimsObjectAsset>(new ResourceKey(0x158, _groupID, TypeIDs.XOBJ));

        Assert.That(objectAsset, Is.Not.Null);
    }
}