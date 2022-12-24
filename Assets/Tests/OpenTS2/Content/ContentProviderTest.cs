using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;

public class ContentProviderTest
{
    [Test]
    public void GetAssetByGlobalTGITest()
    {
        TestMain.Initialize();
        var contentProvider = ContentProvider.Get();
        contentProvider.AddPackage("TestAssets/TestPackage.package");
        var stringAsset = contentProvider.GetAsset<StringSetAsset>(new ResourceKey(1, "testpackage", TypeIDs.STR));
        Assert.IsNotNull(stringAsset);
    }
}
