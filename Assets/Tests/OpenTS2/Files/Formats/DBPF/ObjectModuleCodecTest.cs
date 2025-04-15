using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class ObjectModuleCodecTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        _groupID = ContentManager.Instance.AddPackage("TestAssets/Codecs/ObjCodecs.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsObjectModule()
    {
        var objectModuleAsset = ContentManager.Instance
            .GetAsset<ObjectModuleAsset>(new ResourceKey(0x1, _groupID, TypeIDs.OBJM));

        Assert.That(objectModuleAsset, Is.Not.Null);
    }
}