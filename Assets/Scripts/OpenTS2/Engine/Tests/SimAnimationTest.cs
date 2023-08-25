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

        private readonly Dictionary<string, ScenegraphAnimationAsset> _animations = new Dictionary<string, ScenegraphAnimationAsset>();
        private Animation _animationObj;
        private SimCharacterComponent _sim;

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
            _animationObj.Play(animName);
        }

        private ScenegraphAnimationAsset UpdateAnimationsListAndGetSelection()
        {
            var matchedAnimations = _animations.Where(pair => pair.Key.StartsWith(animationName));
            var tenMatchedAnimations = matchedAnimations.Take(10).ToArray();

            // Display 10 matching animations.
            matchedNames = string.Join("\n", tenMatchedAnimations.Select(pair => pair.Key));
            return tenMatchedAnimations.Length == 0 ? null : tenMatchedAnimations[0].Value;
        }
    }
}