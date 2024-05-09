using System;
using System.Collections.Generic;
using OpenTS2.Common.Utils;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Formats.DBPF.Types;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphAnimationAsset : AbstractAsset
    {
        public AnimResourceConstBlock AnimResource { get; }

        public ScenegraphAnimationAsset(AnimResourceConstBlock animResource)
        {
            AnimResource = animResource;
        }

        public AnimationClip CreateClipFromResource(Dictionary<string, string> bonesToRelativePaths,
            Dictionary<string, List<string>> blendsToRelativePaths)
        {
            var clip = new AnimationClip();
            // mark as legacy for now, this might need to change when we do IK animations.
            clip.legacy = true;

            foreach (var target in AnimResource.AnimTargets)
            {
                foreach (var channel in target.Channels)
                {
                    if (channel.AnimatedAttribute == AnimResourceConstBlock.AnimatedAttribute.ContactIk)
                    {
                        // This is called "contact" but it controls ik weight.
                        CreateBlendCurveForIkWeight(clip, channel);
                    }
                    else if (bonesToRelativePaths.TryGetValue(channel.ChannelName, out var relativePathToBone))
                    {
                        CreateBoneCurvesForChannel(clip, channel, relativePathToBone);
                    }
                    else if (blendsToRelativePaths.TryGetValue(channel.ChannelName, out var relativePathsToBlend))
                    {
                        foreach (var blendRelativePath in relativePathsToBlend)
                        {
                            CreateBlendCurveForChannel(clip, channel, blendRelativePath);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Bone or blend for animation channel {channel.ChannelName} not found");
                    }
                }
            }

            return clip;
        }

        private static void CreateBlendCurveForIkWeight(AnimationClip clip, AnimResourceConstBlock.SharedChannel channel)
        {
            if (channel.Type != AnimResourceConstBlock.ChannelType.Float1)
            {
                throw new ArgumentException("Invalid channel type for ik weight");
            }

            // TODO: Create a mapping in SimsComponent for the rig name.
            string rigPath = channel.ChannelName switch
            {
                "r_handcontrol0_a" => "ik-rig-4736ACCE/ikChain-4736ACCE-0",
                "r_handcontrol1_b" => "ik-rig-4736ACCE/ikChain-4736ACCE-1",
                "l_handcontrol0_a" => "ik-rig-D0C343C9/ikChain-D0C343C9-0",
                "l_handcontrol1_b" => "ik-rig-D0C343C9/ikChain-D0C343C9-1",
                _ => channel.ChannelName
            };

            var curve = new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[0]));
            var property = $"auskel/UnityAnimationRigs/{rigPath}";
            clip.SetCurve(property, typeof(TwoBoneIKConstraint), "m_Data.m_TargetPositionWeight", curve);
        }

        public static void CreateBlendCurveForChannel(AnimationClip clip, AnimResourceConstBlock.SharedChannel channel,
            string relativePathToBlend)
        {
            if (channel.Type != AnimResourceConstBlock.ChannelType.Float1)
            {
                throw new ArgumentException("Invalid channel type for blend shape animation");
            }

            // unity blend shapes are from 0 to 99, whereas sims has them as 0 to 1.0
            // Multiply each keyframe value by 100.
            var originalKeyFrames = CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[0]);
            var keyFrames = new Keyframe[originalKeyFrames.Length];
            for (var i = 0; i < originalKeyFrames.Length; i++)
            {
                keyFrames[i] = new Keyframe(originalKeyFrames[i].time, originalKeyFrames[i].value * 100,
                    originalKeyFrames[i].inTangent * 100, originalKeyFrames[i].outTangent * 100);
            }

            var curve = new AnimationCurve(keyFrames);
            var property = $"blendShape.{channel.ChannelName}";
            clip.SetCurve(relativePathToBlend, typeof(SkinnedMeshRenderer), property, curve);
        }

        private static void CreateBoneCurvesForChannel(AnimationClip clip, AnimResourceConstBlock.SharedChannel channel,
            string relativePathToBone)
        {
            if (channel.Type == AnimResourceConstBlock.ChannelType.EulerRotation)
            {
                var xFrames = CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[0]);
                var yFrames = CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[1]);
                var zFrames = CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[2]);
                var curveX = new AnimationCurve(xFrames);
                var curveY = new AnimationCurve(yFrames);
                var curveZ = new AnimationCurve(zFrames);

                var quatX = new Keyframe[xFrames.Length];
                var quatY = new Keyframe[xFrames.Length];
                var quatZ = new Keyframe[xFrames.Length];
                var quatW = new Keyframe[xFrames.Length];

                // Convert from xyz euler rotations to quaternions.
                for (var i = 0; i < curveX.keys.Length; i++)
                {
                    var xFrame = curveX.keys[i];
                    var t = xFrame.time;
                    var (y, z) = (curveY.Evaluate(t), curveZ.Evaluate(t));
                    var quat = QuaternionSerializer.FromSims2EulerRotationDegrees(xFrame.value, y, z);

                    quatX[i] = new Keyframe(t, quat.x);
                    quatY[i] = new Keyframe(t, quat.y);
                    quatZ[i] = new Keyframe(t, quat.z);
                    quatW[i] = new Keyframe(t, quat.w);
                }

                curveX = new AnimationCurve(quatX);
                curveY = new AnimationCurve(quatY);
                curveZ = new AnimationCurve(quatZ);
                var curveW = new AnimationCurve(quatW);

                clip.SetCurve(relativePathToBone, typeof(Transform), "localRotation.x", curveX);
                clip.SetCurve(relativePathToBone, typeof(Transform), "localRotation.y", curveY);
                clip.SetCurve(relativePathToBone, typeof(Transform), "localRotation.z", curveZ);
                clip.SetCurve(relativePathToBone, typeof(Transform), "localRotation.w", curveW);
            }
            else if (channel.Type == AnimResourceConstBlock.ChannelType.TransformXYZ)
            {
                var curveX =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[0]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localPosition.x", curveX);
                var curveY =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[1]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localPosition.y", curveY);
                var curveZ =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[2]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localPosition.z", curveZ);
            }
            else if (channel.Type == AnimResourceConstBlock.ChannelType.Float1)
            {
                Debug.Assert(channel.NumComponents == 1,
                    $"{channel.ChannelName} is Float1 and has {channel.NumComponents} components");
                var curve = new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[0]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localPosition.x", curve);
                clip.SetCurve(relativePathToBone, typeof(Transform), "localPosition.y", curve);
                clip.SetCurve(relativePathToBone, typeof(Transform), "localPosition.z", curve);
            }
            else
            {
                throw new NotImplementedException($"Unsupported channel type: {channel.Type}");
            }
        }

        private static Keyframe[] CreateKeyFramesForComponent(uint durationTicks,
            AnimResourceConstBlock.ChannelComponent component)
        {
            var unityKeyframes = new Keyframe[component.KeyFrames.Length];

            for (var i = 0; i < component.KeyFrames.Length; i++)
            {
                switch (component.KeyFrames[i])
                {
                    case IKeyFrame.BakedKeyFrame keyFrame:
                        // If this animation has baked data then the time steps are evenly distributed
                        // across its duration.
                        var keyFrameTimeEven =
                            ConvertTimeToSeconds(durationTicks * (((float)i) / component.NumKeyFrames));

                        unityKeyframes[i] = new Keyframe(keyFrameTimeEven, keyFrame.Data);
                        break;
                    case IKeyFrame.ContinuousKeyFrame keyFrame:
                        // TODO: fix tangentin and tangentout values.
                        unityKeyframes[i] = new Keyframe(ConvertTimeToSeconds(keyFrame.Time), keyFrame.Data,
                            GetTangentIn(i, component.KeyFrames), GetTangentOut(i, component.KeyFrames));
                        break;
                    case IKeyFrame.DiscontinuousKeyFrame keyFrame:
                        unityKeyframes[i] = new Keyframe(ConvertTimeToSeconds(keyFrame.Time), keyFrame.Data,
                            GetTangentIn(i, component.KeyFrames), GetTangentOut(i, component.KeyFrames));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return unityKeyframes;
        }

        // Assuming 24fps for now.
        private const float TicksToSeconds = (float)(AnimResourceConstBlock.FramesPerTick / 24.0);

        private static float ConvertTimeToSeconds(float timeInTicks)
        {
            return timeInTicks * TicksToSeconds;
        }

        private static float GetTangentIn(int frameIdx, IReadOnlyList<IKeyFrame> frames)
        {
            if (frameIdx == 0 || frames.Count == 1)
                return 0f;

            switch (frames[frameIdx])
            {
                case IKeyFrame.ContinuousKeyFrame keyFrame:
                    var previousContFrame = (IKeyFrame.ContinuousKeyFrame)frames[frameIdx - 1];
                    return keyFrame.TangentIn *
                           (ConvertTimeToSeconds(keyFrame.Time) - ConvertTimeToSeconds(previousContFrame.Time)) *
                           TicksToSeconds;
                case IKeyFrame.DiscontinuousKeyFrame keyFrame:
                    var previousDiscontFrame = (IKeyFrame.DiscontinuousKeyFrame)frames[frameIdx - 1];
                    return keyFrame.TangentIn *
                           (ConvertTimeToSeconds(keyFrame.Time) - ConvertTimeToSeconds(previousDiscontFrame.Time)) *
                           TicksToSeconds;
            }

            throw new ArgumentOutOfRangeException($"Invalid frame type: {frames[frameIdx]}");
        }

        private static float GetTangentOut(int frameIdx, IReadOnlyList<IKeyFrame> frames)
        {
            if (frameIdx == frames.Count - 1 || frames.Count == 1)
                return 0f;

            switch (frames[frameIdx])
            {
                case IKeyFrame.ContinuousKeyFrame _:
                    // The tangent-out is just the next frame's tangentIn.
                    return GetTangentIn(frameIdx + 1, frames);
                case IKeyFrame.DiscontinuousKeyFrame keyFrame:
                    var nextDiscontFrame = (IKeyFrame.DiscontinuousKeyFrame)frames[frameIdx + 1];
                    return keyFrame.TangentOut *
                           (ConvertTimeToSeconds(nextDiscontFrame.Time) - ConvertTimeToSeconds(keyFrame.Time)) *
                           TicksToSeconds;
            }

            throw new ArgumentOutOfRangeException($"Invalid frame type: {frames[frameIdx]}");
        }
    }
}