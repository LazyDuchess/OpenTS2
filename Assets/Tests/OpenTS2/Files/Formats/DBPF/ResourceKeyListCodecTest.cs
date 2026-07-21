using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class ResourceKeyListCodecTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        _groupID = ContentManager.Instance.AddPackage("TestAssets/Codecs/SimData.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsResourceKeyList()
    {
        var keyListAsset = ContentManager.Instance
            .GetAsset<ResourceKeyListAsset>(new ResourceKey(1, _groupID, TypeIDs.RES_KEY_LIST));

        Assert.IsNotNull(keyListAsset);
        Assert.AreEqual(14, keyListAsset.Keys.Count);

        var first = keyListAsset.Keys[0];
        Assert.AreEqual(0xE519C933, first.TypeID);
        Assert.AreEqual(0xFFFFFFFF, first.GroupID);
        Assert.AreEqual(0xFF9E6E9F, first.InstanceID);
        Assert.AreEqual(0x7A123A08, first.InstanceHigh);

        var last = keyListAsset.Keys[13];
        Assert.AreEqual(0xEBCF3E27, last.TypeID);
        Assert.AreEqual(0x2C17B74A, last.GroupID);
        Assert.AreEqual(0xAD52585B, last.InstanceID);
        Assert.AreEqual(0x00000000, last.InstanceHigh);
    }
}
