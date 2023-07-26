using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

public class ScenegraphResourceNodeCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Scenegraph/teapot_model.package");
    }

    [Test]
    public void LoadsResourceNode()
    {
        var node = ContentProvider.Get()
            .GetAsset<ScenegraphResourceAsset>(new ResourceKey("ufoCrash_cres", 0x1C0532FA, TypeIDs.SCENEGRAPH_CRES));

        var shapeRef = node.ResourceCollection.GetBlockOfType<ShapeRefNodeBlock>();
        Assert.That(shapeRef.BlockTypeInfo.Name, Is.EqualTo("cShapeRefNode"));

        var transformNode = shapeRef.Renderable.Bounded.Transform;
        Assert.That(transformNode.Transform, Is.EqualTo(new Vector3(0, 0, 0)));
        Assert.That(transformNode.Rotation, Is.EqualTo(new Quaternion(0, 0, 0, 1)));
        Assert.That(transformNode.BoneId, Is.EqualTo(0));

        var renderableNode = shapeRef.Renderable;
        Assert.That(renderableNode.RenderGroups, Is.EquivalentTo(new[]{"Practical"}));
        Assert.That(renderableNode.AddToDisplayList, Is.EqualTo(true));

        // Full white with full alpha.
        Assert.That(shapeRef.ShapeColor, Is.EqualTo(0xFF_FF_FF_FF));
        Assert.That(shapeRef.MorphChannelNames, Is.EquivalentTo(new string[]{}));
        Assert.That(shapeRef.MorphChannelWeights, Is.EquivalentTo(new float[]{}));
    }
}