using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;

public class ScenegraphAnimationCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Scenegraph/animation.package");
    }

    [Test]
    public void TestLoadsAnimationNode()
    {
        var animationAsset = ContentProvider.Get()
            .GetAsset<ScenegraphAnimationAsset>(new ResourceKey("a2o-pinball-play-lose_anim", GroupIDs.Scenegraph,
                TypeIDs.SCENEGRAPH_ANIM));
        Assert.IsNotNull(animationAsset);
    }

    [Test]
    public void TestLoadsAnimationAndHasCorrectChannels()
    {
        var animationAsset = ContentProvider.Get()
            .GetAsset<ScenegraphAnimationAsset>(new ResourceKey("o-vehiclePizza-driveOff_anim", GroupIDs.Scenegraph,
                TypeIDs.SCENEGRAPH_ANIM));
        Assert.IsNotNull(animationAsset);
    }
}