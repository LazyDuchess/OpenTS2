using System;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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
            const string bodyResourceName = "amBodyNaked_cres";
            var bodyAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(bodyResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            const string baldHairResourceName = "amHairBald_cres";
            var baldHairAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(baldHairResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            const string baseFaceResourceName = "amFace_cres";
            var baseFaceAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(baseFaceResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));

            // Create a scenegraph with all 3 resources.
            var simsObject =
                ScenegraphComponent.CreateRootScenegraph(new[] { bodyAsset, baldHairAsset, baseFaceAsset });
            var scenegraph = simsObject.GetComponentInChildren<ScenegraphComponent>();

            // Add a bone-renderer so we can visualize the sim's bones.
            var boneRenderer = scenegraph.gameObject.AddComponent<BoneRenderer>();
            var boneTransforms = new List<Transform>();
            GetAllBoneTransformsForVisualization(scenegraph.BoneNamesToTransform["root_trans"], boneTransforms);
            boneRenderer.transforms = boneTransforms.ToArray();

            // Parent the scenegraph to the created SimCharacterComponent.
            var simCharacterObject = new GameObject("sim_character", typeof(SimCharacterComponent));
            simsObject.transform.parent = simCharacterObject.transform;
            return simCharacterObject.GetComponent<SimCharacterComponent>();
        }

        private static void GetAllBoneTransformsForVisualization(Transform rootBone, ICollection<Transform> bones)
        {
            for (var i = 0; i < rootBone.childCount; i++)
            {
                var bone = rootBone.GetChild(i);
                // Ignore "surface" and "grip" transforms, they just make the skeleton look messy.
                if (bone.name.Contains("surface") || bone.name.Contains("grip"))
                {
                    continue;
                }
                bones.Add(bone);
                GetAllBoneTransformsForVisualization(bone, bones);
            }
        }
    }
}