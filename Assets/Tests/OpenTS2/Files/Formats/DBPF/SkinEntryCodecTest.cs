using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class SkinEntryCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        ContentManager.Instance.AddPackage("TestAssets/Codecs/SimData.package");
    }

    [Test]
    public void TestParsesShapeKey()
    {
        var skinEntryAsset = ContentManager.Instance
            .GetAsset<SkinEntryAsset>(new ResourceKey(0xF55A9384, 0x2C17B74A, TypeIDs.SKIN_ENTRY));
        Assert.IsNotNull(skinEntryAsset.ShapeResourceKey);
        Assert.IsInstanceOf<ResourceKeyIndexProp>(skinEntryAsset.ShapeResourceKey);

        Assert.That(skinEntryAsset.MaterialOverrides.Count, Is.EqualTo(4));
        Assert.That(skinEntryAsset.MaterialOverrides[0].SubsetName, Is.EqualTo("hair_alpha5"));
        Assert.That(skinEntryAsset.MaterialOverrides[1].SubsetName, Is.EqualTo("hair_alpha3"));
        Assert.That(skinEntryAsset.MaterialOverrides[2].SubsetName, Is.EqualTo("hat"));
        Assert.That(skinEntryAsset.MaterialOverrides[3].SubsetName, Is.EqualTo("hair"));
    }
}
