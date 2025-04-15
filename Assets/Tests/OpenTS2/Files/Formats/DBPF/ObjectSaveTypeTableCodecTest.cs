using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class ObjectSaveTypeTableCodecTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        _groupID = ContentManager.Instance.AddPackage("TestAssets/Codecs/ObjCodecs.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsSaveTypeTable()
    {
        var tableAsset = ContentManager.Instance
            .GetAsset<ObjectSaveTypeTableAsset>(new ResourceKey(0x0, _groupID, TypeIDs.OBJ_SAVE_TYPE_TABLE));

        Assert.That(tableAsset, Is.Not.Null);

        var firstSelector = tableAsset.Selectors[0];
        Assert.That(firstSelector.objectGuid, Is.EqualTo(0xD20C0AEE));
        Assert.That(firstSelector.saveType, Is.EqualTo(1));
        Assert.That(firstSelector.catalogResourceName, Is.EqualTo("*Trash Can - Outside"));
    }
}