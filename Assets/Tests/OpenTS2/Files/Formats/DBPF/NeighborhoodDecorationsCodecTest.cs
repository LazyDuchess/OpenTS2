using System.Linq;
using NUnit.Framework;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class NeighborhoodDecorationsCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Codecs/NeighborhoodDecorations.package");
    }

    [Test]
    public void TestSuccessfullyLoadsFirstTree()
    {
        var decorationsAsset = ContentProvider.Get()
            .GetAssetsOfType<NeighborhoodDecorationsAsset>(TypeIDs.NHOOD_DECORATIONS).Single();

        Assert.That(decorationsAsset.FloraDecorations.Length, Is.EqualTo(44));

        var firstTree = decorationsAsset.FloraDecorations[0];
        Assert.That(firstTree.ObjectId, Is.EqualTo(0x18));

        Assert.That(firstTree.Position.Position.x, Is.EqualTo(395.625).Within(0.05));
        Assert.That(firstTree.Position.Position.z, Is.EqualTo(535.725).Within(0.05));
        Assert.That(firstTree.Position.Position.y, Is.EqualTo(357.551).Within(0.05));
        Assert.That(firstTree.Position.Rotation, Is.EqualTo(0));

        Assert.That(firstTree.Position.BoundingBoxMin.x, Is.EqualTo(392.56).Within(0.05));
        Assert.That(firstTree.Position.BoundingBoxMin.y, Is.EqualTo(535.052).Within(0.05));

        Assert.That(firstTree.Position.BoundingBoxMax.x, Is.EqualTo(399.706).Within(0.05));
        Assert.That(firstTree.Position.BoundingBoxMax.y, Is.EqualTo(538.4807).Within(0.05));
    }
}