using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

public class ScenegraphAnimationCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        ContentManager.Instance.AddPackage("TestAssets/Scenegraph/animation.package");
    }

    [Test]
    public void TestLoadsAnimationNode()
    {
        var animationAsset = ContentManager.Instance
            .GetAsset<ScenegraphAnimationAsset>(new ResourceKey("a2o-pinball-play-lose_anim", GroupIDs.Scenegraph,
                TypeIDs.SCENEGRAPH_ANIM));
        Assert.IsNotNull(animationAsset);

        Assert.That(animationAsset.AnimResource.LocomotionType, Is.EqualTo(0));
        Assert.That(animationAsset.AnimResource.HeadingOffset, Is.EqualTo(0.0));
        Assert.That(animationAsset.AnimResource.TurnRotation, Is.EqualTo(0.0));
        Assert.That(animationAsset.AnimResource.LocomotionDistance, Is.EqualTo(0.0));
        Assert.That(animationAsset.AnimResource.Velocity, Is.EqualTo(0.0));
    }

    [Test]
    public void TestLoadsAnimationAndHasCorrectChannels()
    {
        var animationAsset = ContentManager.Instance
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
        Assert.That(translationChannel.IKChainIdx, Is.EqualTo(-1)); // No ik chain for this channel.
        Assert.That(translationChannel.AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.Transform));
        Assert.That(translationChannel.IsBaseDataReturned, Is.EqualTo(true));
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
        Assert.That(bodyRotation.IKChainIdx, Is.EqualTo(-1)); // No ik chain for this channel.
        Assert.That(bodyRotation.AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.Rotation));
        Assert.That(bodyRotation.IsBaseDataReturned, Is.EqualTo(true));
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

    private static AnimResourceConstBlock.SharedChannel[] GetChannelsByName(AnimResourceConstBlock.AnimTarget target, string name)
    {
        var channels = target.Channels.Where(c => c.ChannelName == name).ToArray();

        if (channels.Length == 0)
        {
            throw new KeyNotFoundException($"Channel {name} not found");
        }

        return channels;
    }

    [Test]
    public void TestInverseKinematicChainChannelsAreCorrect()
    {
        var animationAsset = ContentManager.Instance
            .GetAsset<ScenegraphAnimationAsset>(new ResourceKey("a2o-exerciseMachine-benchPress-start_anim", GroupIDs.Scenegraph,
                TypeIDs.SCENEGRAPH_ANIM));

        Assert.That(animationAsset.AnimResource.IsFullyBodySkeletal, Is.EqualTo(true));
        Assert.That(animationAsset.AnimResource.LocomotionType, Is.EqualTo(0));
        Assert.That(animationAsset.AnimResource.HeadingOffset, Is.EqualTo(-1.570).Within(0.001));
        Assert.That(animationAsset.AnimResource.TurnRotation, Is.EqualTo(0.0));
        Assert.That(animationAsset.AnimResource.LocomotionDistance, Is.EqualTo(0.0));
        Assert.That(animationAsset.AnimResource.Velocity, Is.EqualTo(0.0));

        var skeletonTarget = animationAsset.AnimResource.AnimTargets[0];
        Assert.That(skeletonTarget.TagName, Is.EqualTo("auskel"));
        Assert.That(skeletonTarget.AnimType, Is.EqualTo(1));

        var rootTransChannels = GetChannelsByName(skeletonTarget, "root_trans");
        Assert.That(rootTransChannels.Length, Is.EqualTo(2));
        Assert.That(rootTransChannels[0].AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.ContactIk));
        Assert.That(rootTransChannels[0].Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.Float1));
        Assert.That(rootTransChannels[0].IsBaseDataReturned, Is.EqualTo(true));
        Assert.That(rootTransChannels[1].AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.Transform));
        Assert.That(rootTransChannels[1].Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.TransformXYZ));
        Assert.That(rootTransChannels[1].IsBaseDataReturned, Is.EqualTo(true));

        var leftHandChannels = GetChannelsByName(skeletonTarget, "l_handcontrol0");
        Assert.That(leftHandChannels.Length, Is.EqualTo(2));
        Assert.That(leftHandChannels[0].AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.Rotation));
        Assert.That(leftHandChannels[0].Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.EulerRotation));
        Assert.That(leftHandChannels[0].IsBaseDataReturned, Is.EqualTo(true));
        Assert.That(leftHandChannels[1].AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.Transform));
        Assert.That(leftHandChannels[1].Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.TransformXYZ));
        Assert.That(leftHandChannels[1].IsBaseDataReturned, Is.EqualTo(true));

        var leftHandContactChannels = GetChannelsByName(skeletonTarget, "l_handcontrol0_a");
        Assert.That(leftHandContactChannels.Length, Is.EqualTo(1));
        Assert.That(leftHandContactChannels[0].AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.ContactIk));
        Assert.That(leftHandContactChannels[0].Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.Float1));

        var leftHandSecondContactChannels = GetChannelsByName(skeletonTarget, "l_handcontrol1_b");
        Assert.That(leftHandSecondContactChannels.Length, Is.EqualTo(1));
        Assert.That(leftHandSecondContactChannels[0].AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.ContactIk));
        Assert.That(leftHandSecondContactChannels[0].Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.Float1));

        var leftHandChannelsNoOffset = GetChannelsByName(skeletonTarget, "l_handcontrol1_noxyzoffset");
        Assert.That(leftHandChannelsNoOffset.Length, Is.EqualTo(1));
        Assert.That(leftHandChannelsNoOffset[0].AnimatedAttribute, Is.EqualTo(AnimResourceConstBlock.AnimatedAttribute.Transform));
        Assert.That(leftHandChannelsNoOffset[0].Type, Is.EqualTo(AnimResourceConstBlock.ChannelType.TransformXYZ));
    }

    [Test]
    public void TestLoadsInverseKinematicChains()
    {
        var animationAsset = ContentManager.Instance
            .GetAsset<ScenegraphAnimationAsset>(new ResourceKey("a2o-pinball-play-lose_anim", GroupIDs.Scenegraph,
                TypeIDs.SCENEGRAPH_ANIM));

        var skeletonTarget = animationAsset.AnimResource.AnimTargets[0];
        Assert.That(skeletonTarget.TagName, Is.EqualTo("auskel"));

        Assert.That(skeletonTarget.IKChains.Length, Is.EqualTo(4));

        var ikChain1 = skeletonTarget.IKChains[0];
        Assert.That(ikChain1.IkStrategy, Is.EqualTo(AnimResourceConstBlock.IKStrategy.SevenDegreesOfFreedom));
        Assert.That(ikChain1.BeginBoneCrc, Is.EqualTo(FileUtils.HighHash("r_thigh")));
        Assert.That(ikChain1.BeginBoneMirrorCrc, Is.EqualTo(FileUtils.HighHash("l_thigh")));
        Assert.That(ikChain1.EndBoneCrc, Is.EqualTo(FileUtils.HighHash("r_foot")));
        Assert.That(ikChain1.EndBoneMirrorCrc, Is.EqualTo(FileUtils.HighHash("l_foot")));
        Assert.That(ikChain1.TwistVectorCrc, Is.EqualTo(FileUtils.HighHash("r_foot_ikpole")));
        Assert.That(ikChain1.TwistVectorBoneCrc, Is.EqualTo(0));
        Assert.That(ikChain1.TwistVectorMirrorBoneCRC, Is.EqualTo(0));
        Assert.That(ikChain1.IkWeightCRC, Is.EqualTo(0));

        Assert.That(ikChain1.IkTargets.Length, Is.EqualTo(1));
        var ikTarget1 = ikChain1.IkTargets[0];
        Assert.That(ikTarget1.BoneCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.BoneMirrorCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.RotationCrc, Is.EqualTo(FileUtils.HighHash("r_foot_ikctr")));
        Assert.That(ikTarget1.Rotation2Crc, Is.EqualTo(0));
        Assert.That(ikTarget1.TranslationCrc, Is.EqualTo(FileUtils.HighHash("r_foot_ikctr")));
        Assert.That(ikTarget1.ContactCrc, Is.EqualTo(0));

        var ikChain2 = skeletonTarget.IKChains[1];
        Assert.That(ikChain2.IkStrategy, Is.EqualTo(AnimResourceConstBlock.IKStrategy.SevenDegreesOfFreedom));
        Assert.That(ikChain2.BeginBoneCrc, Is.EqualTo(FileUtils.HighHash("l_thigh")));
        Assert.That(ikChain2.BeginBoneMirrorCrc, Is.EqualTo(FileUtils.HighHash("r_thigh")));
        Assert.That(ikChain2.EndBoneCrc, Is.EqualTo(FileUtils.HighHash("l_foot")));
        Assert.That(ikChain2.EndBoneMirrorCrc, Is.EqualTo(FileUtils.HighHash("r_foot")));
        Assert.That(ikChain2.TwistVectorCrc, Is.EqualTo(FileUtils.HighHash("l_foot_ikpole")));
        Assert.That(ikChain2.TwistVectorBoneCrc, Is.EqualTo(0));
        Assert.That(ikChain2.TwistVectorMirrorBoneCRC, Is.EqualTo(0));
        Assert.That(ikChain2.IkWeightCRC, Is.EqualTo(0));

        Assert.That(ikChain2.IkTargets.Length, Is.EqualTo(1));
        ikTarget1 = ikChain2.IkTargets[0];
        Assert.That(ikTarget1.BoneCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.BoneMirrorCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.RotationCrc, Is.EqualTo(FileUtils.HighHash("l_foot_ikctr")));
        Assert.That(ikTarget1.Rotation2Crc, Is.EqualTo(0));
        Assert.That(ikTarget1.TranslationCrc, Is.EqualTo(FileUtils.HighHash("l_foot_ikctr")));
        Assert.That(ikTarget1.ContactCrc, Is.EqualTo(0));

        var ikChain3 = skeletonTarget.IKChains[2];
        Assert.That(ikChain3.IkStrategy, Is.EqualTo(AnimResourceConstBlock.IKStrategy.LookAt));
        Assert.That(ikChain3.BeginBoneCrc, Is.EqualTo(FileUtils.HighHash("head")));
        Assert.That(ikChain3.BeginBoneMirrorCrc, Is.EqualTo(FileUtils.HighHash("head")));
        Assert.That(ikChain3.EndBoneCrc, Is.EqualTo(FileUtils.HighHash("head_grip")));
        Assert.That(ikChain3.EndBoneMirrorCrc, Is.EqualTo(FileUtils.HighHash("head_grip")));
        Assert.That(ikChain3.TwistVectorCrc, Is.EqualTo(FileUtils.HighHash("head_ikpole")));
        Assert.That(ikChain3.TwistVectorBoneCrc, Is.EqualTo(0));
        Assert.That(ikChain3.TwistVectorMirrorBoneCRC, Is.EqualTo(0));
        Assert.That(ikChain3.IkWeightCRC, Is.EqualTo(0));

        Assert.That(ikChain3.IkTargets.Length, Is.EqualTo(1));
        ikTarget1 = ikChain3.IkTargets[0];
        Assert.That(ikTarget1.BoneCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.BoneMirrorCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.RotationCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.Rotation2Crc, Is.EqualTo(0));
        Assert.That(ikTarget1.TranslationCrc, Is.EqualTo(FileUtils.HighHash("head_ikctr")));
        Assert.That(ikTarget1.ContactCrc, Is.EqualTo(0));

        var ikChain4 = skeletonTarget.IKChains[3];
        Assert.That(ikChain4.IkStrategy, Is.EqualTo(AnimResourceConstBlock.IKStrategy.SevenDegreesOfFreedom));
        Assert.That(ikChain4.BeginBoneCrc, Is.EqualTo(FileUtils.HighHash("l_upperarm")));
        Assert.That(ikChain4.BeginBoneMirrorCrc, Is.EqualTo(FileUtils.HighHash("r_upperarm")));
        Assert.That(ikChain4.EndBoneCrc, Is.EqualTo(FileUtils.HighHash("l_hand")));
        Assert.That(ikChain4.EndBoneMirrorCrc, Is.EqualTo(FileUtils.HighHash("r_hand")));
        Assert.That(ikChain4.TwistVectorCrc, Is.EqualTo(FileUtils.HighHash("l_hand_ikpole")));
        Assert.That(ikChain4.TwistVectorBoneCrc, Is.EqualTo(0));
        Assert.That(ikChain4.TwistVectorMirrorBoneCRC, Is.EqualTo(0));
        Assert.That(ikChain4.IkWeightCRC, Is.EqualTo(FileUtils.HighHash("l_handcontrol0")));

        Assert.That(ikChain4.IkTargets.Length, Is.EqualTo(1));
        ikTarget1 = ikChain4.IkTargets[0];
        Assert.That(ikTarget1.BoneCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.BoneMirrorCrc, Is.EqualTo(0));
        Assert.That(ikTarget1.RotationCrc, Is.EqualTo(FileUtils.HighHash("l_handcontrol0")));
        Assert.That(ikTarget1.Rotation2Crc, Is.EqualTo(FileUtils.HighHash("l_handcontrol0rot")));
        Assert.That(ikTarget1.TranslationCrc, Is.EqualTo(FileUtils.HighHash("l_handcontrol0")));
        Assert.That(ikTarget1.ContactCrc, Is.EqualTo(FileUtils.HighHash("l_handcontrol0_a")));
    }

    [Test]
    public void LoadsInverseKinematicChainsWithMultipleTargets()
    {
        var animationAsset = ContentManager.Instance
            .GetAsset<ScenegraphAnimationAsset>(new ResourceKey("a2o-exerciseMachine-benchPress-start_anim", GroupIDs.Scenegraph,
                TypeIDs.SCENEGRAPH_ANIM));

        var skeletonTarget = animationAsset.AnimResource.AnimTargets[0];
        Assert.That(skeletonTarget.TagName, Is.EqualTo("auskel"));

        Assert.That(skeletonTarget.IKChains.Length, Is.EqualTo(4));

        var ikChain3 = skeletonTarget.IKChains[2];
        Assert.That(ikChain3.IkStrategy, Is.EqualTo(AnimResourceConstBlock.IKStrategy.SevenDegreesOfFreedom));
        Assert.That(ikChain3.IkTargets.Length, Is.EqualTo(2));
        Assert.That(ikChain3.BeginBoneCrc, Is.EqualTo(FileUtils.HighHash("r_upperarm")));

        var ikTarget1 = ikChain3.IkTargets[0];
        Assert.That(ikTarget1.TranslationCrc, Is.EqualTo(FileUtils.HighHash("r_handcontrol0")));
        Assert.That(ikTarget1.ContactCrc, Is.EqualTo(FileUtils.HighHash("r_handcontrol0_a")));
        var ikTarget2 = ikChain3.IkTargets[1];
        Assert.That(ikTarget2.TranslationCrc, Is.EqualTo(FileUtils.HighHash("r_handcontrol1_noxyzoffset")));
        Assert.That(ikTarget2.ContactCrc, Is.EqualTo(FileUtils.HighHash("r_handcontrol1_b")));


        var ikChain4 = skeletonTarget.IKChains[3];
        Assert.That(ikChain4.IkStrategy, Is.EqualTo(AnimResourceConstBlock.IKStrategy.SevenDegreesOfFreedom));
        Assert.That(ikChain4.IkTargets.Length, Is.EqualTo(2));
        Assert.That(ikChain4.BeginBoneCrc, Is.EqualTo(FileUtils.HighHash("l_upperarm")));

        ikTarget1 = ikChain4.IkTargets[0];
        Assert.That(ikTarget1.TranslationCrc, Is.EqualTo(FileUtils.HighHash("l_handcontrol0")));
        Assert.That(ikTarget1.ContactCrc, Is.EqualTo(FileUtils.HighHash("l_handcontrol0_a")));
        ikTarget2 = ikChain4.IkTargets[1];
        Assert.That(ikTarget2.TranslationCrc, Is.EqualTo(FileUtils.HighHash("l_handcontrol1_noxyzoffset")));
        Assert.That(ikTarget2.ContactCrc, Is.EqualTo(FileUtils.HighHash("l_handcontrol1_b")));
    }
}