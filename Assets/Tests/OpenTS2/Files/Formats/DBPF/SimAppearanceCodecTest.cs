using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class SimAppearanceCodecTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        _groupID = ContentManager.Instance.AddPackage("TestAssets/Codecs/SimData.package").GroupID;
    }

    [Test]
    public void TestSuccessfullyLoadsAppearanceObject()
    {
        var simAppearanceAsset = ContentManager.Instance
            .GetAsset<SimAppearanceAsset>(new ResourceKey(1, _groupID, TypeIDs.SIM_APPEARANCE));

        Assert.IsNotNull(simAppearanceAsset);
        Assert.AreEqual(simAppearanceAsset.Stretch, 0.94, delta:.01);
    }

    [Test]
    public void TestParsesEntryLists()
    {
        var simAppearanceAsset = ContentManager.Instance
            .GetAsset<SimAppearanceAsset>(new ResourceKey(1, _groupID, TypeIDs.SIM_APPEARANCE));

        Assert.AreEqual(32, simAppearanceAsset.EntryLists.Count);

        var entries = simAppearanceAsset.EntryLists[new OutfitCategoryKey(OutfitType.Casual1, OutfitCategory.Hair)];
        Assert.AreEqual(1, entries.Count);
        Assert.IsInstanceOf<ResourceKeyIndexProp>(entries[0]);
        Assert.AreEqual(3, ((ResourceKeyIndexProp)entries[0]).Value);

        var sleepBodyEntries = simAppearanceAsset.EntryLists[new OutfitCategoryKey(OutfitType.Sleep, OutfitCategory.Body)];
        Assert.AreEqual(1, sleepBodyEntries.Count);
        Assert.AreEqual(8, ((ResourceKeyIndexProp)sleepBodyEntries[0]).Value);
    }
}