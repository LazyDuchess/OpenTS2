using System;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
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
            // Load the skeleton, body, hair and face resources.
            const string skeletonResourceName = "auskel_cres";
            var skeletonAsset = ContentProvider.Get().GetAsset<ScenegraphResourceAsset>(
                new ResourceKey(skeletonResourceName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_CRES));
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
                ScenegraphComponent.CreateRootScenegraph(new[] { skeletonAsset, bodyAsset, baldHairAsset, baseFaceAsset });
            var scenegraph = simsObject.GetComponentInChildren<ScenegraphComponent>();

            var simCharacterObject = new GameObject("sim_character", typeof(SimCharacterComponent));
            // Parent the scenegraph to the created SimCharacterComponent.
            simsObject.transform.parent = simCharacterObject.transform;

            var simsComponent = simCharacterObject.GetComponent<SimCharacterComponent>();
            simsComponent.SetupAnimationRig(scenegraph);

            return simsComponent;
        }

        public ScenegraphComponent Scenegraph { get; private set; }

        /// <summary>
        /// This is the unity object that all the animation rigging constraints get placed into.
        /// </summary>
        private GameObject _animationRig;
        private RigBuilder _rigBuilder;
        /// <summary>
        /// Set of IK chains that have already been applied to this sim so they don't get re-applied.
        /// </summary>
        private readonly HashSet<uint> _appliedIKChains = new HashSet<uint>();

        private void SetupAnimationRig(ScenegraphComponent scene)
        {
            Scenegraph = scene;
            var skeleton = Scenegraph.BoneNamesToTransform["auskel"].gameObject;

            // Add a bone-renderer so we can visualize the sim's bones.
            var boneRenderer = skeleton.AddComponent<BoneRenderer>();
            var boneTransforms = new List<Transform>();
            GetAllBoneTransformsForVisualization(Scenegraph.BoneNamesToTransform["root_trans"], boneTransforms);
            boneRenderer.transforms = boneTransforms.ToArray();

            // Add an animator component to the auskel.
            var animator = skeleton.AddComponent<Animator>();
            // Add a rig builder.
            _rigBuilder = skeleton.AddComponent<RigBuilder>();

            // Add a child rig to the skeleton.
            _animationRig = new GameObject("UnityAnimationRig", typeof(Rig));
            _animationRig.transform.SetParent(skeleton.transform, worldPositionStays:false);

            _rigBuilder.layers.Add(new RigLayer(_animationRig.GetComponent<Rig>()));
            _rigBuilder.layers[0].Initialize(animator);
            _rigBuilder.Build();
        }

        public void AddInverseKinematicsFromAnimation(AnimResourceConstBlock anim)
        {
            AnimResourceConstBlock.AnimTarget auSkelTarget = null;
            foreach (var target in anim.AnimTargets)
            {
                if (target.TagName.ToLower() == "auskel")
                {
                    auSkelTarget = target;
                }
            }

            if (auSkelTarget == null)
            {
                return;
            }
            AddInverseKinematicsFromIKChains(auSkelTarget.IKChains);
        }

        private void AddInverseKinematicsFromIKChains(IEnumerable<AnimResourceConstBlock.IKChain> ikChains)
        {
            foreach (var chain in ikChains)
            {
                if (_appliedIKChains.Contains(chain.NameCrc))
                {
                    continue;
                }

                Debug.Assert(chain.BeginBoneCrc != 0, "ikChain without a beginning bone");
                Debug.Assert(chain.EndBoneCrc != 0, "ikChain without an end bone");
                if (!Scenegraph.BoneCRC32ToTransform.ContainsKey(chain.BeginBoneCrc))
                {
                    Debug.LogWarning($"begin bone crc from ik chain {chain.BeginBoneCrc:X} not found");
                    continue;
                }
                if (!Scenegraph.BoneCRC32ToTransform.ContainsKey(chain.EndBoneCrc))
                {
                    Debug.LogWarning($"end bone crc from ik chain {chain.EndBoneCrc:X} not found");
                    continue;
                }

                Debug.Assert(chain.NumIkTargets == 1, "ikChain with more than 1 target");
                var target = chain.IkTargets[0];
                Debug.Assert(target.TranslationCrc == target.RotationCrc, "ikTarget with different translation and rotation objects");
                if (!Scenegraph.BoneCRC32ToTransform.ContainsKey(target.TranslationCrc))
                {
                    Debug.LogWarning($"ik target {target.TranslationCrc:X} not found");
                    continue;
                }

                var root = Scenegraph.BoneCRC32ToTransform[chain.BeginBoneCrc];
                var tip = Scenegraph.BoneCRC32ToTransform[chain.EndBoneCrc];

                GameObject ikChainObj;
                if (tip.parent.parent == root)
                {
                    Debug.Assert(chain.TwistVectorCrc != 0, "Two bone ik chain without twist vector");

                    ikChainObj = new GameObject($"ikChain{chain.NameCrc:X}", typeof(TwoBoneIKConstraint));
                    var twoBoneConstraint = new TwoBoneIKConstraintData
                    {
                        root = root,
                        mid = tip.parent,
                        tip = tip,
                        target = Scenegraph.BoneCRC32ToTransform[target.TranslationCrc],
                        targetPositionWeight = 1.0f,
                        targetRotationWeight = 0.0f,
                        hintWeight = 1.0f,
                    };

                    if (chain.TwistVectorCrc != 0)
                    {
                        twoBoneConstraint.hint = Scenegraph.BoneCRC32ToTransform[chain.TwistVectorCrc];
                    }
                    ikChainObj.GetComponent<TwoBoneIKConstraint>().data = twoBoneConstraint;
                }
                else
                {
                    ikChainObj = new GameObject($"ikChain{chain.NameCrc:X}", typeof(ChainIKConstraint));
                    var chainConstraint = new ChainIKConstraintData
                    {
                        root = Scenegraph.BoneCRC32ToTransform[chain.BeginBoneCrc],
                        tip = Scenegraph.BoneCRC32ToTransform[chain.EndBoneCrc],
                        chainRotationWeight = 0.5f,
                        tipRotationWeight = 0.0f,
                        target = Scenegraph.BoneCRC32ToTransform[target.TranslationCrc],
                    };
                    ikChainObj.GetComponent<ChainIKConstraint>().data = chainConstraint;
                }


                ikChainObj.transform.parent = _animationRig.transform;
                _appliedIKChains.Add(chain.NameCrc);
            }

            _rigBuilder.Build();
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