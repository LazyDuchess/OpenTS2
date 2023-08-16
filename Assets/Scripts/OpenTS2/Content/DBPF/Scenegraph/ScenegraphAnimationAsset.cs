using System;
using System.Collections.Generic;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphAnimationAsset : AbstractAsset
    {
        public AnimResourceConstBlock AnimResource { get; }

        public ScenegraphAnimationAsset(AnimResourceConstBlock animResource)
        {
            AnimResource = animResource;
        }

        public AnimationClip CreateClipFromResource(Dictionary<string, string> bonesToRelativePaths, Dictionary<string, List<string>> blendsToRelativePaths)
        {
            var clip = new AnimationClip();
            // mark as legacy for now, this might need to change when we do IK animations.
            clip.legacy = true;

            foreach (var target in AnimResource.AnimTargets)
            {
                foreach (var channel in target.Channels)
                {
                    if (bonesToRelativePaths.TryGetValue(channel.ChannelName, out var relativePathToBone))
                    {
                        CreateBoneCurvesForChannel(clip, channel, relativePathToBone);
                    } else if (blendsToRelativePaths.TryGetValue(channel.ChannelName, out var relativePathsToBlend))
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

        private static void CreateBlendCurveForChannel(AnimationClip clip, AnimResourceConstBlock.SharedChannel channel,
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
                    originalKeyFrames[i].inTangent, originalKeyFrames[i].outTangent);
            }

            var curve = new AnimationCurve(keyFrames);
            var property = $"blendShape.{channel.ChannelName}";
            clip.SetCurve(relativePathToBlend, typeof(SkinnedMeshRenderer), property, curve);
        }

        private static void CreateBoneCurvesForChannel(AnimationClip clip, AnimResourceConstBlock.SharedChannel channel, string relativePathToBone)
        {
            if (channel.Type == AnimResourceConstBlock.ChannelType.EulerRotation)
            {
                var curveX =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[0]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localEulerAngles.x", curveX);
                var curveY =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[1]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localEulerAngles.y", curveY);
                var curveZ =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[2]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localEulerAngles.z", curveZ);
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
                // If this animation has baked data or continuous tangents then the time steps are evenly distributed
                // across its duration. Discontinuous tangents can have a time step anywhere.
                var keyFrameTimeEven = ConvertTimeToSeconds(durationTicks * (((float)i) / component.NumKeyFrames));

                switch (component.KeyFrames[i])
                {
                    // TODO: re-enable tangentIn and tangentOut once they're figured out.
                    case IKeyFrame.BakedKeyFrame keyFrame:
                        unityKeyframes[i] = new Keyframe(keyFrameTimeEven,  keyFrame.Data);
                        break;
                    case IKeyFrame.ContinuousKeyFrame keyFrame:
                        // TODO: these tangentin and tangentout values are based on the original gradient where the x-axis
                        // is in ticks. We use seconds for the x-axis so the gradients need to be changed accordingly.
                        unityKeyframes[i] = new Keyframe(keyFrameTimeEven, keyFrame.Data/*,
                            GetTangentIn(i, component.KeyFrames), GetTangentOut(i, component.KeyFrames)*/);
                        break;
                    case IKeyFrame.DiscontinuousKeyFrame keyFrame:
                        var time = (float)keyFrame.Time;
                        // TODO: fix in and out tangents for discontinuous frames.
                        unityKeyframes[i] = new Keyframe(ConvertTimeToSeconds(time), keyFrame.Data/*,
                            GetTangentIn(i, component.KeyFrames), GetTangentOut(i, component.KeyFrames)*/);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return unityKeyframes;
        }

        // Assuming 30fps for now.
        private const float TicksToSeconds = (float)(AnimResourceConstBlock.FramesPerTick / 30.0);

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
                    return keyFrame.TangentOut * (keyFrame.TangentIn - previousContFrame.TangentIn);
                case IKeyFrame.DiscontinuousKeyFrame keyFrame:
                    var previousDiscontFrame = (IKeyFrame.DiscontinuousKeyFrame)frames[frameIdx - 1];
                    return keyFrame.TangentOut * (keyFrame.Time - previousDiscontFrame.Time);
            }

            throw new ArgumentOutOfRangeException($"Invalid frame type: {frames[frameIdx]}");
        }

        private static float GetTangentOut(int frameIdx, IReadOnlyList<IKeyFrame> frames)
        {
            if (frameIdx == frames.Count - 1 || frames.Count == 1)
                return 0f;

            switch (frames[frameIdx])
            {
                case IKeyFrame.ContinuousKeyFrame keyFrame:
                    var nextContFrame = (IKeyFrame.ContinuousKeyFrame)frames[frameIdx + 1];
                    return keyFrame.TangentOut * (nextContFrame.TangentIn - keyFrame.TangentIn);
                case IKeyFrame.DiscontinuousKeyFrame keyFrame:
                    var nextDiscontFrame = (IKeyFrame.DiscontinuousKeyFrame)frames[frameIdx + 1];
                    return keyFrame.TangentOut * (nextDiscontFrame.Time - keyFrame.Time);
            }

            throw new ArgumentOutOfRangeException($"Invalid frame type: {frames[frameIdx]}");
        }
    }
}