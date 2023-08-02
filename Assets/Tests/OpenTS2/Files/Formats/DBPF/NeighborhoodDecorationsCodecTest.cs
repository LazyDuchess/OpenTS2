using System.Linq;
using NUnit.Framework;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

public class NeighborhoodDecorationsCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Codecs/NeighborhoodDecorations.package");
    }

    [Test]
    public void TestSuccessfullyLoadsDecorations()
    {
        var decorationsAsset = ContentProvider.Get()
            .GetAssetsOfType<NeighborhoodDecorationsAsset>(TypeIDs.NHOOD_DECORATIONS).Single();

        Assert.That(decorationsAsset.FloraDecorations.Length, Is.EqualTo(1208));

        var firstTree = decorationsAsset.FloraDecorations[0];
        Assert.That(firstTree.ObjectId, Is.EqualTo(0x53));

        Assert.That(firstTree.Position.Position.x, Is.EqualTo(1025.75).Within(0.05));
        Assert.That(firstTree.Position.Position.z, Is.EqualTo(1106.39).Within(0.05));
        Assert.That(firstTree.Position.Position.y, Is.EqualTo(313.78).Within(0.05));
        Assert.That(firstTree.Rotation, Is.EqualTo(0));

        Assert.That(firstTree.Position.BoundingBoxMin.x, Is.EqualTo(1023.35).Within(0.05));
        Assert.That(firstTree.Position.BoundingBoxMin.y, Is.EqualTo(1103.99).Within(0.05));

        Assert.That(firstTree.Position.BoundingBoxMax.x, Is.EqualTo(1028.15).Within(0.05));
        Assert.That(firstTree.Position.BoundingBoxMax.y, Is.EqualTo(1108.78).Within(0.05));


        Assert.That(decorationsAsset.RoadDecorations.Length, Is.EqualTo(367));
        var firstRoad = decorationsAsset.RoadDecorations[0];
        Assert.That(firstRoad.PieceId, Is.EqualTo(0x4b00));
        Assert.That(firstRoad.UnderTextureId, Is.EqualTo(0));
        Assert.That(firstRoad.Flags, Is.EqualTo(0x01));
        Assert.That(firstRoad.ConnectionFlag, Is.EqualTo(0x0A));
        Assert.That(firstRoad.GetTextureName("new_roads_{0}_txtr"), Is.EqualTo("new_roads_00004b04_txtr"));


        Assert.That(decorationsAsset.BridgeDecorations.Length, Is.EqualTo(10));
        var firstBridge = decorationsAsset.BridgeDecorations[0];
        Assert.That(firstBridge.ResourceName, Is.EqualTo("0ac3-bridge_cres"));


        Assert.That(decorationsAsset.PropDecorations.Length, Is.EqualTo(102));
    }
}