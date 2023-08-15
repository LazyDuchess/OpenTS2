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

        // Third channel is the car body's translation.
        var translationChannel = animTarget.Channels[2];
        Assert.That(translationChannel.ChannelName, Is.EqualTo("body_trans"));
        Assert.That(translationChannel.Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.TransformXYZ));
        // Since we have an XYZ animation, there should be one component per axis.
        Assert.That(translationChannel.Components.Length, Is.EqualTo(3));
        Assert.That(translationChannel.DurationTicks, Is.EqualTo(286));
        // The root translation has a fixed X component, so we expect a single keyframe there.
        Assert.That(translationChannel.Components[0].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.BakedTangents));
        Assert.That(translationChannel.Components[0].KeyFrames.Length, Is.EqualTo(1));
        // The root translation has a fixed Y component, so we expect a single keyframe there.
        Assert.That(translationChannel.Components[1].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.BakedTangents));
        Assert.That(translationChannel.Components[1].KeyFrames.Length, Is.EqualTo(1));
        Assert.That(translationChannel.Components[1].KeyFrames[0], Is.TypeOf<IKeyFrame.BakedKeyFrame>());
        var rootTranslationYKeyFrame = (IKeyFrame.BakedKeyFrame)translationChannel.Components[1].KeyFrames[0];
        Assert.That(rootTranslationYKeyFrame.Data, Is.EqualTo(0.0));

        // The Z component actually has some keyframed data.
        Assert.That(translationChannel.Components[2].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.ContinuousTangents));
        Assert.That(translationChannel.Components[2].KeyFrames.Length, Is.EqualTo(3));
        Assert.That(translationChannel.Components[2].KeyFrames[0], Is.TypeOf<IKeyFrame.ContinuousKeyFrame>());
        var rootTransZKeyFrames = translationChannel.Components[2].KeyFrames;
        Assert.That(((IKeyFrame.ContinuousKeyFrame) rootTransZKeyFrames[0]).Data, Is.EqualTo(0.930).Within(0.01));
        Assert.That(((IKeyFrame.ContinuousKeyFrame) rootTransZKeyFrames[1]).Data, Is.EqualTo(0.935).Within(0.01));
        Assert.That(((IKeyFrame.ContinuousKeyFrame) rootTransZKeyFrames[2]).Data, Is.EqualTo(0.929).Within(0.01));

        // The fourth channel is the car body's rotation.
        var bodyRotation = animTarget.Channels[3];
        Assert.That(bodyRotation.ChannelName, Is.EqualTo("body_rot"));
        Assert.That(bodyRotation.Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.EulerRotation));
        // One component per axis: roll, pitch, yaw.
        Assert.That(bodyRotation.Components.Length, Is.EqualTo(3));
        Assert.That(bodyRotation.DurationTicks, Is.EqualTo(286));
        Assert.That(bodyRotation.Components[0].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.ContinuousTangents));
        Assert.That(bodyRotation.Components[0].KeyFrames.Length, Is.EqualTo(4));
        Assert.That(bodyRotation.Components[1].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.ContinuousTangents));
        Assert.That(bodyRotation.Components[1].KeyFrames.Length, Is.EqualTo(3));
        Assert.That(bodyRotation.Components[2].TangentCurveType,
            Is.EqualTo(AnimResourceConstBlock.ChannelComponent.CurveType.BakedTangents));
        Assert.That(bodyRotation.Components[2].KeyFrames.Length, Is.EqualTo(1));
    }
}