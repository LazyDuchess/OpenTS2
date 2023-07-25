using System.Collections.Generic;
using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;

public class ScenegraphShapeCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Scenegraph/teapot_model.package");
    }

    [Test]
    public void TestLoadsShapeNode()
    {
        var node = ContentProvider.Get()
            .GetAsset<ScenegraphShapeAsset>(new ResourceKey("ufoCrash_ufo_shpe", 0x1C0532FA, TypeIDs.SCENEGRAPH_SHPE));

        Assert.That(node.ShapeBlock.Resource.ResourceName, Is.EqualTo("ufoCrash_ufo_shpe"));

        Assert.That(node.ShapeBlock.LodLevels.Length, Is.EqualTo(1));
        Assert.That(node.ShapeBlock.LodLevels, Is.EquivalentTo(ShapeBlockReader.LODLevels));

        Assert.That(node.ShapeBlock.MeshesPerLod.Count, Is.EqualTo(1));
        Assert.That(node.ShapeBlock.MeshesPerLod, Contains.Key(0));
        Assert.That(node.ShapeBlock.MeshesPerLod[0], Is.EquivalentTo(new[] { "ufoCrash_tslocator_gmnd" }));

        var expectedMaterials = new Dictionary<string, string>()
        {
            { "ufocrash_body", "ufocrash_body" },
            { "ufocrash_cabin", "ufocrash_cabin" },
            { "neighborhood_roundshadow", "neighborhood_roundshadow" }
        };
        Assert.That(node.ShapeBlock.Materials, Is.EquivalentTo(expectedMaterials));
    }
}