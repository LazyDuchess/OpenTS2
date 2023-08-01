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
    public void TestSuccessfullyLoadsRegularLot()
    {
        var lotInfoAsset = ContentProvider.Get()
            .GetAsset<LotInfoAsset>(new ResourceKey(0x3, _groupID, TypeIDs.LOT_INFO));

        Assert.That(lotInfoAsset.LotId, Is.EqualTo(3));

        Assert.That(lotInfoAsset.LotName, Is.EqualTo("1 Tesla Court"));
        Assert.That(lotInfoAsset.LotDescription, Is.EqualTo("1 Tesla Court"));

        Assert.That(lotInfoAsset.LocationX, Is.EqualTo(54));
        Assert.That(lotInfoAsset.LocationY, Is.EqualTo(71));
        Assert.That(lotInfoAsset.NeighborhoodToLotHeightOffset, Is.EqualTo(365.43).Within(0.01));
        Assert.That(lotInfoAsset.FrontEdge, Is.EqualTo(0));

        Assert.That(lotInfoAsset.HasRoadAlongEdge(LotEdge.NegativeX));
    }

    [Test]
    public void TestSuccessfullyLoadsBusinessLot()
    {
        var lotInfoAsset = ContentProvider.Get()
            .GetAsset<LotInfoAsset>(new ResourceKey(0x22, _groupID, TypeIDs.LOT_INFO));

        Assert.That(lotInfoAsset.LotId, Is.EqualTo(0x22));

        Assert.That(lotInfoAsset.LotName, Is.EqualTo("153 Main Street"));
        Assert.That(lotInfoAsset.LotDescription, Is.EqualTo(""));

        Assert.That(lotInfoAsset.LocationX, Is.EqualTo(41));
        Assert.That(lotInfoAsset.LocationY, Is.EqualTo(76));
        Assert.That(lotInfoAsset.NeighborhoodToLotHeightOffset, Is.EqualTo(318.75).Within(0.01));
        Assert.That(lotInfoAsset.FrontEdge, Is.EqualTo(3));

        Assert.That(lotInfoAsset.HasRoadAlongEdge(LotEdge.NegativeZ));
    }
}