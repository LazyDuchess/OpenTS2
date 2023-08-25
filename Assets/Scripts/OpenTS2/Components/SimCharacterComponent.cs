using System;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace OpenTS2.Components
{
    /// <summary>
    /// This component represents a rendered out sims character with their head, hair and body meshes in place under one
    /// scenegraph component.
    /// </summary>
    [RequireComponent(typeof(AssetReferenceComponent))]
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
                ScenegraphComponent.CreateRootScenegraph(skeletonAsset, bodyAsset, baldHairAsset, baseFaceAsset);
            var scenegraph = simsObject.GetComponentInChildren<ScenegraphComponent>();

            var simCharacterObject = new GameObject("sim_character", typeof(SimCharacterComponent));
            // Parent the scenegraph to the created SimCharacterComponent.
            simsObject.transform.parent = simCharacterObject.transform;

            // Hold references to the scenegraph resources we use.
            simCharacterObject.GetComponent<AssetReferenceComponent>().AddReference(skeletonAsset, bodyAsset, baldHairAsset, baseFaceAsset);

            var simsComponent = simCharacterObject.GetComponent<SimCharacterComponent>();
            simsComponent.SetupAnimationRig(scenegraph);

            return simsComponent;
        }

        public ScenegraphComponent Scenegraph { get; private set; }

        /// <summary>
        /// This is the unity object that all the animation rigs get placed into.
        /// </summary>
        private GameObject _animationRigs;
        private Animator _animator;
        private RigBuilder _rigBuilder;

        /// <summary>
        /// Set of IK chains that have already been applied to this sim so they don't get re-applied.
        /// </summary>
        private readonly Dictionary<uint, RigLayer> _appliedIKChains = new Dictionary<uint, RigLayer>();

        /// <summary>
        /// These are basically parent bones of IK chains, this is needed so that if an animation updates the position
        /// or rotation of these, unity's IK system responds and moves appropriately.
        /// </summary>
        private static readonly string[] BonesThatUpdateIK =
        {
            "root_trans", "root_rot", "pelvis", "spine0", "spine1", "spine2", "r_clavicle", "l_clavicle",
            "l_handcontrol0", "r_handcontrol0"
        };

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
            _animator = skeleton.AddComponent<Animator>();
            // Add a rig builder.
            _rigBuilder = skeleton.AddComponent<RigBuilder>();

            // Add a child rig to the skeleton.
            _animationRigs = new GameObject("UnityAnimationRigs");
            _animationRigs.transform.SetParent(skeleton.transform, worldPositionStays: false);

            // HACK: In the auskel model, for some reason the bones that represent the IK goals for the hands are
            // parented to the root_trans of the skeleton. This is done even though in animations the IK goal bones
            // get moved relative to the model root, NOT root_trans.
            //
            // We re-parent them here to the base model to avoid this issue.
            Scenegraph.BoneNamesToTransform["l_handcontrol0"].SetParent(skeleton.transform, worldPositionStays:true);
            Scenegraph.BoneNamesToTransform["r_handcontrol0"].SetParent(skeleton.transform, worldPositionStays:true);
            Scenegraph.BoneNamesToRelativePaths["l_handcontrol0"]    = "auskel/l_handcontrol0";
            Scenegraph.BoneNamesToRelativePaths["l_handcontrol0rot"] = "auskel/l_handcontrol0/l_handcontrol0rot";
            Scenegraph.BoneNamesToRelativePaths["r_handcontrol0"]    = "auskel/r_handcontrol0";
            Scenegraph.BoneNamesToRelativePaths["r_handcontrol0rot"] = "auskel/r_handcontrol0/r_handcontrol0rot";
            // HACK.

            // Add RigTransforms to the sim's root translation and rotation bones. This allows the unity rigging
            // animation system to move the legs to the proper IK goals if the rest of the skeleton moves.
            foreach (var ikUpdatingBone in BonesThatUpdateIK)
            {
                Scenegraph.BoneNamesToTransform[ikUpdatingBone].gameObject.AddComponent<RigTransform>();
            }

            AddGizmosAroundInverseKinmaticsPositions();

            //_rigBuilder.layers.Add(new RigLayer(_animationRig.GetComponent<Rig>()));
            //_rigBuilder.layers[0].Initialize(animator);
            _rigBuilder.Build();
        }

        private void AddGizmosAroundInverseKinmaticsPositions()
        {
            // Add some effectors around the foot control points to see what animations should look like.
            var cubePrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cubeMesh = cubePrimitive.GetComponent<MeshFilter>().sharedMesh;
            _rigBuilder.AddEffector(Scenegraph.BoneNamesToTransform["l_foot_ikctr"],
                new RigEffectorData.Style()
                    { color = new Color(1.0f, 0.0f, 0.0f, 0.5f), size = 0.05f, shape = cubeMesh });
            _rigBuilder.AddEffector(Scenegraph.BoneNamesToTransform["r_foot_ikctr"],
                new RigEffectorData.Style()
                    { color = new Color(0.0f, 1.0f, 0.0f, 0.5f), size = 0.05f, shape = cubeMesh });
            Destroy(cubePrimitive);

            var spherePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var sphereMesh = cubePrimitive.GetComponent<MeshFilter>().sharedMesh;
            _rigBuilder.AddEffector(Scenegraph.BoneNamesToTransform["l_handcontrol0"],
                new RigEffectorData.Style()
                    { color = new Color(1.0f, 0.0f, 0.0f, 0.5f), size = 0.05f, shape = sphereMesh });
            _rigBuilder.AddEffector(Scenegraph.BoneNamesToTransform["l_hand_ikpole"],
                new RigEffectorData.Style()
                    { color = new Color(0.7f, 0.2f, 0.0f, 0.5f), size = 0.04f, shape = sphereMesh });
            _rigBuilder.AddEffector(Scenegraph.BoneNamesToTransform["r_handcontrol0"],
                new RigEffectorData.Style()
                    { color = new Color(0.0f, 1.0f, 0.0f, 0.5f), size = 0.05f, shape = sphereMesh });
            _rigBuilder.AddEffector(Scenegraph.BoneNamesToTransform["r_hand_ikpole"],
                new RigEffectorData.Style()
                    { color = new Color(0.0f, 0.8f, 0.4f, 0.5f), size = 0.04f, shape = sphereMesh });
            Destroy(spherePrimitive);
        }

        public void AdjustInverseKinematicWeightsForAnimation(AnimResourceConstBlock anim)
        {
            // TODO: This should eventually live in the animation controller for the sim. This is here for easy testing
            // for now.
            var auSkelTarget = anim.AnimTargets.FirstOrDefault(t => t.TagName.ToLower() == "auskel");
            if (auSkelTarget == null)
            {
                return;
            }
            AddInverseKinematicsFromAnimation(auSkelTarget);

            // Disable all current rigs.
            foreach (var rigLayer in _appliedIKChains.Values)
            {
                rigLayer.active = false;
            }
            // Activate the ones present in the current animation.
            foreach (var chain in auSkelTarget.IKChains)
            {
                if (!_appliedIKChains.TryGetValue(chain.NameCrc, out var rigLayer))
                {
                    continue;
                }
                rigLayer.active = true;
            }
        }

        private void AddInverseKinematicsFromAnimation(AnimResourceConstBlock.AnimTarget auSkelTarget)
        {
            AddInverseKinematicsFromIKChains(auSkelTarget.IKChains);
        }

        private void AddInverseKinematicsFromIKChains(IEnumerable<AnimResourceConstBlock.IKChain> ikChains)
        {
            foreach (var chain in ikChains)
            {
                if (_appliedIKChains.ContainsKey(chain.NameCrc))
                {
                    continue;
                }
                var rigObject = new GameObject($"ik-rig-{chain.NameCrc:X}", typeof(Rig));
                rigObject.transform.SetParent(_animationRigs.transform);
                var rig = rigObject.GetComponent<Rig>();

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
                Debug.Assert(target.TranslationCrc == target.RotationCrc,
                    "ikTarget with different translation and rotation objects");
                if (!Scenegraph.BoneCRC32ToTransform.ContainsKey(target.TranslationCrc))
                {
                    Debug.LogWarning($"ik target {target.TranslationCrc:X} not found");
                    continue;
                }

                var root = Scenegraph.BoneCRC32ToTransform[chain.BeginBoneCrc];
                var tip = Scenegraph.BoneCRC32ToTransform[chain.EndBoneCrc];
                // Find the mid bone between root and tip.
                var middleBone = FindTransformInTheMiddleOf(tip, root);

                var ikChainObj = new GameObject($"ikChain{chain.NameCrc:X}", typeof(TwoBoneIKConstraint));
                var twoBoneConstraint = new TwoBoneIKConstraintData
                {
                    root = root,
                    mid = middleBone,
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
                ikChainObj.transform.parent = rigObject.transform;

                if (target.Rotation2Crc != 0)
                {
                    var rotationConstraint =
                        new GameObject($"ikChain{chain.NameCrc:X}-rotation", typeof(OverrideTransform));
                    var rotationOverrideConstraint = new OverrideTransformData
                    {
                        constrainedObject = tip,
                        sourceObject = Scenegraph.BoneCRC32ToTransform[target.Rotation2Crc],
                        rotationWeight = 1.0f,
                        positionWeight = 0.0f,
                        space = OverrideTransformData.Space.Local,
                    };
                    rotationConstraint.GetComponent<OverrideTransform>().data = rotationOverrideConstraint;
                    rotationConstraint.transform.parent = rigObject.transform;
                }

                var rigLayer = new RigLayer(rig);
                _rigBuilder.layers.Add(rigLayer);
                rigLayer.Initialize(_animator);
                _appliedIKChains[chain.NameCrc] = rigLayer;
            }

            _rigBuilder.Build();
        }

        private static Transform FindTransformInTheMiddleOf(Transform tip, Transform root)
        {
            var numBones = 0;
            var boneFinder = tip;
            while (boneFinder != root)
            {
                boneFinder = boneFinder.parent;
                numBones++;
            }

            Debug.Assert(numBones > 1, $"No bone between {tip} and {root}");

            var middle = tip;
            for (var i = 0; i < numBones / 2; i++)
            {
                middle = middle.parent;
            }

            return middle;
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