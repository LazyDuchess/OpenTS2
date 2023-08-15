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

        public AnimationClip CreateClipFromResource(Dictionary<string, string> bonesToRelativePaths)
        {
            var clip = new AnimationClip();
            // mark as legacy for now, this might need to change when we do IK animations.
            clip.legacy = true;

            foreach (var target in AnimResource.AnimTargets)
            {
                foreach (var channel in target.Channels)
                {
                    var relativePathToBone = bonesToRelativePaths[channel.ChannelName];
                    CreateCurvesForChannel(clip, channel, relativePathToBone);
                }
            }

            return clip;
        }

        private static void CreateCurvesForChannel(AnimationClip clip, AnimResourceConstBlock.SharedChannel channel, string relativePathToBone)
        {
            if (channel.Type == AnimResourceConstBlock.ChannelType.EulerRotation)
            {
                var curveX =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[0]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localRotation.x", curveX);
                var curveY =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[1]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localRotation.y", curveY);
                var curveZ =
                    new AnimationCurve(CreateKeyFramesForComponent(channel.DurationTicks, channel.Components[2]));
                clip.SetCurve(relativePathToBone, typeof(Transform), "localRotation.z", curveZ);
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
                    case IKeyFrame.BakedKeyFrame keyFrame:
                        unityKeyframes[i] = new Keyframe(keyFrameTimeEven,  keyFrame.Data);
                        break;
                    case IKeyFrame.ContinuousKeyFrame keyFrame:
                        unityKeyframes[i] = new Keyframe(keyFrameTimeEven, keyFrame.Data, keyFrame.TangentIn, keyFrame.TangentOut);
                        break;
                    case IKeyFrame.DiscontinuousKeyFrame keyFrame:
                        var time = (float)keyFrame.Time;
                        unityKeyframes[i] = new Keyframe(ConvertTimeToSeconds(time), keyFrame.Data, keyFrame.TangentIn, keyFrame.TangentOut);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return unityKeyframes;
        }

        private static float ConvertTimeToSeconds(float timeInTicks)
        {
            return (float)(timeInTicks * AnimResourceConstBlock.FramesPerTick);
        }
    }
}