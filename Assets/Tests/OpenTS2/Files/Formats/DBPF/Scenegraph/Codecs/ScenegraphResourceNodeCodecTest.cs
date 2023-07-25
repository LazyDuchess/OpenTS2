using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;

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
            .GetAsset<ScenegraphShapeAsset>(new ResourceKey("ufoCrash_cres", 0x1C0532FA, TypeIDs.SCENEGRAPH_CRES));
    }
}