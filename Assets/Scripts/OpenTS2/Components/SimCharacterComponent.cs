using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.Components
{
    /// <summary>
    /// This component represents a rendered out sims character with their head, hair and body meshes in place under one
    /// scenegraph component.
    /// </summary>
    public class SimCharacterComponent : MonoBehaviour
    {
        public static SimCharacterComponent CreateNakedBaseSim()
        {
            const string bodyResourceName = "amBodyMadScientist_cres";
            var bodyAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(bodyResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            const string baldHairResourceName = "amHairBald_cres";
            var baldHairAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(baldHairResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            const string baseFaceResourceName = "amFace_cres";
            var baseFaceAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(baseFaceResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            var simsObject =
                ScenegraphComponent.CreateRootScenegraph(new[] { bodyAsset, baldHairAsset, baseFaceAsset });
            var scenegraph = simsObject.GetComponentInChildren<ScenegraphComponent>();

            AddIkComponents(scenegraph);

            var gameObject = new GameObject("sim_character", typeof(SimCharacterComponent));
            simsObject.transform.parent = gameObject.transform;
            return gameObject.GetComponent<SimCharacterComponent>();
        }

        private static void AddIkComponents(ScenegraphComponent scene)
        {
            var animationAsset = ContentProvider.Get()
                .GetAsset<ScenegraphAnimationAsset>(new ResourceKey("a-stairs-up-loop-L-sexy_anim", GroupIDs.Scenegraph,
                    TypeIDs.SCENEGRAPH_ANIM));

            foreach (var ikChain in animationAsset.AnimResource.AnimTargets[0].IKChains)
            {
                // TODO: add the proper rigging constraints here.
            }
        }
    }
}