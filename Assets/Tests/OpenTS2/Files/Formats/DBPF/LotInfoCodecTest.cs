using System.Linq;
using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class LotInfoCodecTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        _groupID = ContentProvider.Get().AddPackage("TestAssets/Codecs/LotInfo.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsDecorations()
    {
        var lotInfoAsset = ContentProvider.Get()
            .GetAsset<LotInfoAsset>(new ResourceKey(0x3, _groupID, TypeIDs.LOT_INFO));

        Assert.NotNull(lotInfoAsset);
    }
}