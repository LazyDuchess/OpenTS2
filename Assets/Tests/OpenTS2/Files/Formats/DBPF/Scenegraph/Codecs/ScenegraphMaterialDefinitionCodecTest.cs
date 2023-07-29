using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;

public class ScenegraphMaterialDefinitionCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Scenegraph/material_definition.package");
    }

    [Test]
    public void TestLoadsMaterialDefinitionSuccessfully()
    {
        var materialAsset = ContentProvider.Get()
            .GetAsset<ScenegraphMaterialDefinitionAsset>(new ResourceKey("ufocrash_cabin_txmt", 0x1C0532FA,
                TypeIDs.SCENEGRAPH_TXMT));

        Assert.That(materialAsset.MaterialDefinition.MaterialName, Is.EqualTo("ufocrash_cabin"));
        Assert.That(materialAsset.MaterialDefinition.Type, Is.EqualTo("StandardMaterial"));

        Assert.That(materialAsset.MaterialDefinition.MaterialProperties.Count, Is.GreaterThan(0));

        Assert.That(materialAsset.MaterialDefinition.MaterialProperties["reflectivity"], Is.EqualTo("0.025"));
        Assert.That(materialAsset.MaterialDefinition.MaterialProperties["stdMatCullMode"], Is.EqualTo("cullClockwise"));
        Assert.That(materialAsset.MaterialDefinition.MaterialProperties["stdMatSpecCoef"],
            Is.EqualTo("0.94,0.94,0.94"));

        Assert.That(materialAsset.MaterialDefinition.TextureNames, Is.EquivalentTo(new[]{ "ufocrash-cabin", }));
    }
}