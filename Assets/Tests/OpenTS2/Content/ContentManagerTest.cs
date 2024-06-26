using NUnit.Framework;
using OpenTS2.Client;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;

public class ContentManagerTest
{
    [Test]
    public void GetAssetByGlobalTGITest()
    {
        TestMain.Initialize();
        var contentManager = ContentManager.Instance;
        contentManager.AddPackage("TestAssets/TestPackage.package");
        var stringAsset = contentManager.GetAsset<StringSetAsset>(new ResourceKey(1, "testpackage", TypeIDs.STR));
        Assert.IsNotNull(stringAsset);
    }

    [Test]
    public void ChangesEditAssetTest()
    {
        TestMain.Initialize();
        var contentManager = ContentManager.Instance;
        contentManager.AddPackage("TestAssets/TestPackage.package");
        var stringAsset = contentManager.GetAsset<StringSetAsset>(new ResourceKey(1, "testpackage", TypeIDs.STR));
        Assert.IsNotNull(stringAsset);

        var stringAssetClone = stringAsset.Clone() as StringSetAsset;
        stringAssetClone.StringData.Strings[Languages.USEnglish][0].Value = "Edited Value";
        stringAssetClone.Save();

        stringAsset = contentManager.GetAsset<StringSetAsset>(new ResourceKey(1, "testpackage", TypeIDs.STR));
        Assert.AreEqual(stringAsset.GetString(0), "Edited Value");
    }

    [Test]
    public void ChangesDeleteAssetTest()
    {
        TestMain.Initialize();
        var contentManager = ContentManager.Instance;
        contentManager.AddPackage("TestAssets/TestPackage.package");
        var stringAsset = contentManager.GetAsset<StringSetAsset>(new ResourceKey(1, "testpackage", TypeIDs.STR));
        Assert.IsNotNull(stringAsset);

        stringAsset.Delete();

        stringAsset = contentManager.GetAsset<StringSetAsset>(new ResourceKey(1, "testpackage", TypeIDs.STR));
        Assert.IsNull(stringAsset);
    }
}
