using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;

public class ScenegraphGeometryNodeCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Scenegraph/teapot_model.package");
    }

    [Test]
    public void TestLoadsGeometryNode()
    {
        var geometryNodeAsset = ContentProvider.Get()
            .GetAsset<ScenegraphGeometryNodeAsset>(new ResourceKey("teapot_tslocator_gmnd", 0x1C0532FA,
                TypeIDs.SCENEGRAPH_GMND));

        var realKey = new ResourceKey("teapot_tslocator_gmdc", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_GMDC);
        Assert.That(geometryNodeAsset.GeometryDataContainerKey, Is.EqualTo(realKey));
    }
}