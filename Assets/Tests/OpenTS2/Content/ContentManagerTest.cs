using NUnit.Framework;
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
        var contentManager = ContentManager.Get();
        contentManager.Provider.AddPackage("TestAssets/TestPackage.package");
        var stringAsset = contentManager.Provider.GetAsset<StringSetAsset>(new ResourceKey(1, "testpackage", TypeIDs.STR));
        Assert.IsNotNull(stringAsset);
    }
}
