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
        private void Start()
        {
            var contentProvider = ContentProvider.Get();

            // Load base game assets.
            contentProvider.AddPackages(
                Filesystem.GetPackagesInDirectory(Filesystem.GetDataPathForProduct(ProductFlags.BaseGame) +
                                                  "/Res/Sims3D"));

            var sim = SimCharacterComponent.CreateNakedBaseSim();

            var animationName = "a-male-stairsStraight-up-sexy_anim";
            var anim = contentProvider.GetAsset<ScenegraphAnimationAsset>(
                new ResourceKey(animationName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_ANIM));
            sim.AddInverseKinematicsFromAnimation(anim.AnimResource);

            // Add the test animation.
            var animationObj = sim.GetComponentInChildren<Animation>();
            var clip = anim.CreateClipFromResource(sim.Scenegraph.BoneNamesToRelativePaths, sim.Scenegraph.BlendNamesToRelativePaths);
            animationObj.AddClip(clip, animationName);
        }
    }
}