using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Components;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class SimAnimationTest : MonoBehaviour
    {
        public string animationName = "";

        [TextArea(minLines: 10, maxLines: 10)]
        public string matchedNames = "";

        private bool baking = false;

        private readonly Dictionary<string, ScenegraphAnimationAsset> _animations = new Dictionary<string, ScenegraphAnimationAsset>();
        private Animation _animationObj;
        private SimCharacterComponent _sim;

        private SimCharacterComponent _bakedSim;
        private Animation _bakedAnim;

        private void Start()
        {
            var contentProvider = ContentProvider.Get();

            // Load base game assets.
            contentProvider.AddPackages(
                Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) +
                                                  "/Res/Sims3D"));

            // Load all animations involving auskel and put them in the dictionary.
            foreach (var animationAsset in contentProvider.GetAssetsOfType<ScenegraphAnimationAsset>(TypeIDs.SCENEGRAPH_ANIM))
            {
                var auSkelTarget = animationAsset.AnimResource.AnimTargets.FirstOrDefault(t => t.TagName.ToLower() == "auskel");
                if (auSkelTarget == null)
                {
                    continue;
                }

                _animations[animationAsset.AnimResource.ScenegraphResource.ResourceName] = animationAsset;
            }
            UpdateAnimationsListAndGetSelection();

            // Create another sim that we can bake animations into and make clean for export.
            _bakedSim = SimCharacterComponent.CreateNakedBaseSim(makeAnimationRig:false);
            _bakedSim.gameObject.name = "bakedSim";
            // Remove ikpole, ikctr and handcontrol objects from the baked sim.
            foreach (var bone in _bakedSim.Scenegraph.transform.Find("auskel").GetComponentsInChildren<Transform>())
            {
                Debug.Log($"bone name: {bone.name}");
                if (bone.name.Contains("ikctr") || bone.name.Contains("ikpole"))
                {
                    UnityEngine.Object.DestroyImmediate(bone.gameObject);
                    Debug.Log("Destroyed");
                }
            }
            UnityEngine.Object.DestroyImmediate(_bakedSim.Scenegraph.transform.Find("auskel/root_trans/r_handcontrol0")
                .gameObject);
            UnityEngine.Object.DestroyImmediate(_bakedSim.Scenegraph.transform.Find("auskel/root_trans/l_handcontrol0")
                .gameObject);
            UnityEngine.Object.DestroyImmediate(_bakedSim.Scenegraph.transform.Find("auskel/root_trans/r_handcontrol1")
                .gameObject);
            UnityEngine.Object.DestroyImmediate(_bakedSim.Scenegraph.transform.Find("auskel/root_trans/l_handcontrol1")
                .gameObject);
            UnityEngine.Object.DestroyImmediate(_bakedSim.Scenegraph.transform.Find("auskel/transform")
                .gameObject);
            UnityEngine.Object.DestroyImmediate(_bakedSim.Scenegraph.transform.Find("auskel/transform")
                .gameObject);
            UnityEngine.Object.DestroyImmediate(_bakedSim.Scenegraph.transform.Find("auskel/transform")
                .gameObject);
            _bakedAnim = _bakedSim.Scenegraph.transform.Find("auskel").gameObject.AddComponent<Animation>();

            _sim = SimCharacterComponent.CreateNakedBaseSim();

            const string animationName = "a-pose-neutral-stand_anim";
            var anim = contentProvider.GetAsset<ScenegraphAnimationAsset>(
                new ResourceKey(animationName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_ANIM));
            _sim.AdjustInverseKinematicWeightsForAnimation(anim.AnimResource);

            // Add the test animation.
            _animationObj = _sim.GetComponentInChildren<Animation>();
            var clip = anim.CreateClipFromResource(_sim.Scenegraph.BoneNamesToRelativePaths, _sim.Scenegraph.BlendNamesToRelativePaths);
            _animationObj.AddClip(clip, animationName);

            //const string testAnimation = "a-blowHorn_anim";
            const string testAnimation = "a2o-punchingBag-punch-start_anim";
            anim = contentProvider.GetAsset<ScenegraphAnimationAsset>(
                new ResourceKey(testAnimation, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_ANIM));
            clip = anim.CreateClipFromResource(_sim.Scenegraph.BoneNamesToRelativePaths, _sim.Scenegraph.BlendNamesToRelativePaths);
            _animationObj.AddClip(clip, testAnimation);
            _sim.AdjustInverseKinematicWeightsForAnimation(anim.AnimResource);
        }

        private void OnValidate()
        {
            // Check what new animations match.
            var anim = UpdateAnimationsListAndGetSelection();
            if (anim == null)
            {
                return;
            }
            if (baking)
            {
                return;
            }
            // Play the neutral animation for one second to reset positions to default.
            _animationObj.Play("a-pose-neutral-stand_anim");

            var animName = anim.AnimResource.ScenegraphResource.ResourceName;
            if (_animationObj.GetClip(animName) == null)
            {
                var clip = anim.CreateClipFromResource(_sim.Scenegraph.BoneNamesToRelativePaths,
                    _sim.Scenegraph.BlendNamesToRelativePaths);
                _animationObj.AddClip(clip, animName);
            }

            StartCoroutine(PlayClipFrameDelayed(animName, anim));
        }

        IEnumerator PlayClipFrameDelayed(string animName, ScenegraphAnimationAsset anim)
        {
            yield return new WaitForFixedUpdate();

            _sim.AdjustInverseKinematicWeightsForAnimation(anim.AnimResource);

            yield return BakeAnimationIntoNewAnimation(animName, anim);
        }

        private ScenegraphAnimationAsset UpdateAnimationsListAndGetSelection()
        {
            var matchedAnimations = _animations.Where(pair => pair.Key.StartsWith(animationName));
            var tenMatchedAnimations = matchedAnimations.Take(10).ToArray();

            // Display 10 matching animations.
            matchedNames = string.Join("\n", tenMatchedAnimations.Select(pair => pair.Key));
            return tenMatchedAnimations.Length == 0 ? null : tenMatchedAnimations[0].Value;
        }

        IEnumerator BakeAnimationIntoNewAnimation(string animName, ScenegraphAnimationAsset anim)
        {
            var bakedName = $"{animName}_baked";
            if (_bakedAnim.GetClip(bakedName) != null)
            {
                yield break;
            }
            baking = true;

            var bakedClip = new AnimationClip();
            bakedClip.legacy = true;

            _animationObj.wrapMode = WrapMode.Clamp;
            // Now play the animation frame-by-frame and bake in all the bones.
            _animationObj.Play(animName);
            var animState = _animationObj[animName];

            // Each bone will have position and rotation keyframes for each frame.
            var boneToKeyframes = new Dictionary<string, PositionAndRotationKeyframes>();
            foreach (var bone in _sim.Scenegraph.BoneNamesToRelativePaths.Keys)
            {
                // Don't bother animating the "surface" or "grip" bones, they're parented and don't actually get
                // animated.
                if (bone.Contains("grip") || bone.Contains("surface"))
                {
                    continue;
                }
                boneToKeyframes[bone] = new PositionAndRotationKeyframes();
            }

            animState.time = 0.0f;
            var timeStep = 1.0f / 24; // 24 frames per second. Bake every frame.
            for (var t = 0.0f; t < animState.length; t += timeStep)
            {
                yield return new WaitForFixedUpdate();

                // for each bone in the skeleton, bake in some key frames.
                foreach (var item in _sim.Scenegraph.BoneNamesToTransform)
                {
                    var boneName = item.Key;
                    // Only do bones present in the set.
                    if (!boneToKeyframes.ContainsKey(boneName))
                    {
                        continue;
                    }
                    var boneTransform = item.Value;
                    boneToKeyframes[boneName].BakeInDataFromTransform(animState.time, boneTransform);
                }

                animState.time = t;
            }

            var boneSet = new HashSet<string>();
            foreach (var bone in _bakedSim.Scenegraph.transform.Find("auskel").GetComponentsInChildren<Transform>())
            {
                boneSet.Add(bone.name);
            }

            foreach (var entry in boneToKeyframes)
            {
                if (!boneSet.Contains(entry.Key))
                {
                    continue;
                }
                var relativeBonePath = _sim.Scenegraph.BoneNamesToRelativePaths[entry.Key];
                relativeBonePath = relativeBonePath.Replace("auskel/", "");
                var posAndRot = entry.Value;
                Debug.Log(relativeBonePath);

                bakedClip.SetCurve(relativeBonePath, typeof(Transform), "localPosition.x", new AnimationCurve(posAndRot.PosX.ToArray()));
                bakedClip.SetCurve(relativeBonePath, typeof(Transform), "localPosition.y", new AnimationCurve(posAndRot.PosY.ToArray()));
                bakedClip.SetCurve(relativeBonePath, typeof(Transform), "localPosition.z", new AnimationCurve(posAndRot.PosZ.ToArray()));

                bakedClip.SetCurve(relativeBonePath, typeof(Transform), "localRotation.x", new AnimationCurve(posAndRot.QuatX.ToArray()));
                bakedClip.SetCurve(relativeBonePath, typeof(Transform), "localRotation.y", new AnimationCurve(posAndRot.QuatY.ToArray()));
                bakedClip.SetCurve(relativeBonePath, typeof(Transform), "localRotation.z", new AnimationCurve(posAndRot.QuatZ.ToArray()));
                bakedClip.SetCurve(relativeBonePath, typeof(Transform), "localRotation.w", new AnimationCurve(posAndRot.QuatW.ToArray()));
            }

            // Add curves for blend channels.
            foreach (var target in anim.AnimResource.AnimTargets)
            {
                foreach (var channel in target.Channels)
                {
                    if (!_sim.Scenegraph.BlendNamesToRelativePaths.TryGetValue(channel.ChannelName,
                            out var relativePathsToBlend)) continue;

                    foreach (var blendRelativePath in relativePathsToBlend)
                    {
                        var blendPath = blendRelativePath.Replace("auskel/", "");
                        ScenegraphAnimationAsset.CreateBlendCurveForChannel(bakedClip, channel, blendPath);
                    }
                }
            }

            _bakedAnim.AddClip(bakedClip, bakedName);
            baking = false;

            //UnityEditor.Formats.Fbx.Exporter.ModelExporter.ExportObject("");
        }

        private class PositionAndRotationKeyframes
        {
            internal List<Keyframe> PosX = new List<Keyframe>();
            internal List<Keyframe> PosY = new List<Keyframe>();
            internal List<Keyframe> PosZ = new List<Keyframe>();

            internal List<Keyframe> QuatX = new List<Keyframe>();
            internal List<Keyframe> QuatY = new List<Keyframe>();
            internal List<Keyframe> QuatZ = new List<Keyframe>();
            internal List<Keyframe> QuatW = new List<Keyframe>();

            public void BakeInDataFromTransform(float time, Transform transform)
            {
                AddValue(PosX, time, transform.localPosition.x);
                AddValue(PosY, time, transform.localPosition.y);
                AddValue(PosZ, time, transform.localPosition.z);

                AddValue(QuatX, time, transform.localRotation.x);
                AddValue(QuatY, time, transform.localRotation.y);
                AddValue(QuatZ, time, transform.localRotation.z);
                AddValue(QuatW, time, transform.localRotation.w);
            }

            private static void AddValue(IList<Keyframe> list, float time, float newValue)
            {
                // Only add keyframe if different from previous value.
                if (list.Count > 0 && Mathf.Approximately(list[list.Count - 1].value, newValue))
                {
                    return;
                }
                list.Add(new Keyframe(time, newValue));
            }
        }
    }
}