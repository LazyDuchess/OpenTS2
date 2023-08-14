using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;

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

        var animResource = animationAsset.AnimResource;
        // Only one target, the vehicle.
        Assert.That(animResource.AnimTargets.Length, Is.EqualTo(1));

        var animTarget = animationAsset.AnimResource.AnimTargets[0];
        // There are a total of 12 animation channels.
        Assert.That(animTarget.Channels.Length, Is.EqualTo(12));

        var rootTranslationChannel = animTarget.Channels[0];
        Assert.That(rootTranslationChannel.ChannelName, Is.EqualTo("root_trans"));
        Assert.That(rootTranslationChannel.Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.TransformXYZ));
        // Since we have an XYZ animation, there should be one component per axis.
        Assert.That(rootTranslationChannel.Components.Length, Is.EqualTo(3));
        Assert.That(rootTranslationChannel.DurationTicks, Is.EqualTo(866));
        // The root translation has a fixed X component, so we expect a single keyframe there.
        Assert.That(rootTranslationChannel.Components[0].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.BakedTangents));
        Assert.That(rootTranslationChannel.Components[0].KeyFrames.Length, Is.EqualTo(1));
        // The Y component actually has some interpolated data.
        Assert.That(rootTranslationChannel.Components[1].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.ContinuousTangents));
        Assert.That(rootTranslationChannel.Components[1].KeyFrames.Length, Is.EqualTo(3));
        // The root translation has a fixed Z component, so we expect a single keyframe there.
        Assert.That(rootTranslationChannel.Components[2].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.BakedTangents));
        Assert.That(rootTranslationChannel.Components[2].KeyFrames.Length, Is.EqualTo(1));
    }
}