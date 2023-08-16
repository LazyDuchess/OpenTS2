using System;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block.GeometryData;
using Unity.Collections;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphModelAsset : AbstractAsset
    {
        public Mesh StaticBoundMesh { get; }

        /// <summary>
        /// The different primitives or groups in the model. For example, a bed may have a "bedding" and a "frame"
        /// mesh primitive.
        /// </summary>
        public Dictionary<string, ModelPrimitive> Primitives { get; } = new Dictionary<string, ModelPrimitive>();

        public ScenegraphModelAsset(GeometryDataContainerBlock geometryBlock)
        {
            // Initialize the mesh.
            StaticBoundMesh = new Mesh
            {
                name = geometryBlock.Resource.ResourceName + "_static_bound"
            };
            StaticBoundMesh.SetVertices(geometryBlock.StaticBounds.Vertices);
            StaticBoundMesh.SetTriangles(geometryBlock.StaticBounds.Faces, 0);

            // Initialize each primitive.
            foreach (var primitive in geometryBlock.Primitives)
            {
                Primitives[primitive.Name] = InitializeMeshFromPrimitive(geometryBlock, primitive);
            }
        }

        public override void FreeUnmanagedResources()
        {
            foreach (var prim in Primitives)
            {
                prim.Value.Mesh.Free();
            }

            StaticBoundMesh.Free();
        }

        public class ModelPrimitive
        {
            public Mesh Mesh { get; internal set; }
            /// <summary>
            /// Set to true when the primitive has morph animations or bones.
            /// </summary>
            public bool NeedsSkinnedRenderer { get; internal set; }
            public bool HasBones => BindPoses != null;

            public Matrix4x4[] BindPoses { get; internal set; }
            public ushort[] ScenegraphBoneIds { get; internal set; }
        }

        private ModelPrimitive InitializeMeshFromPrimitive(GeometryDataContainerBlock geometryBlock, MeshPrimitive primitive)
        {
            var modelPrimitive = new ModelPrimitive();
            var mesh = new Mesh
            {
                name = geometryBlock.Resource.ResourceName + "_" + primitive.Name
            };
            modelPrimitive.Mesh = mesh;

            var meshComponent = geometryBlock.GetMeshComponentForPrimitive(primitive);
            var elements = geometryBlock.GetGeometryElementsForMeshComponent(meshComponent);
            var vertices = elements.OfType<VertexElement>().Single();

            mesh.SetVertices(GetVerticesForMeshComponent(vertices, meshComponent));
            mesh.SetTriangles(primitive.Faces, 0);

            BoneAssignmentsElement boneAssignment = null;
            IBoneWeightsElement boneWeights = null;

            foreach (var geometryElement in elements)
            {
                switch (geometryElement)
                {
                    case VertexElement _:
                        // Handled before this loop.
                        break;
                    case NormalElement normals:
                        mesh.SetNormals(GetNormalMapForMeshComponent(normals, meshComponent));
                        break;
                    case TangentElement tangentsElement:
                        // Unity wants Vec4s for the tangents with the last component used to flip the binormal.
                        var tangents = new Vector4[tangentsElement.Data.Length];
                        for (var i = 0; i < tangentsElement.Data.Length; i++)
                        {
                            var tangent = tangentsElement.Data[i];
                            tangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, 1);
                        }

                        mesh.SetTangents(tangents);
                        break;
                    case UVMapElement uvMap:
                        mesh.SetUVs(0, GetUVMapForMeshComponent(uvMap, meshComponent));
                        break;

                    // Set the bone assignment and weights when they show up so they can be added to the mesh.
                    case BoneAssignmentsElement boneAssignmentsElement:
                        boneAssignment = boneAssignmentsElement;
                        break;
                    case IBoneWeightsElement boneWeightsElement:
                        boneWeights = boneWeightsElement;
                        break;

                    // Handled below in the blend animations section.
                    case MorphVertexMapElement _:
                    case MorphNormalDeltaElement _:
                    case MorphVertexPositionDeltaElement _:
                    case MorphNormalIndicesElement _:
                    case MorphVertexPositionIndicesElement _:
                        break;

                    default:
                        Debug.LogWarning($"Unknown geometry element type: {geometryElement.GetType()}");
                        break;
                }
            }

            // Add bones if present.
            if (boneAssignment != null)
            {
                Debug.Assert(boneWeights != null, "Bone weights null when boneAssignments present");
                AssignBones(modelPrimitive, primitive.BoneIndices, geometryBlock.BindPoses, boneAssignment, boneWeights);

                modelPrimitive.NeedsSkinnedRenderer = true;
            }

            // Add morph animations if present.
            var vertexMap = elements.OfType<MorphVertexMapElement>().ToArray();
            if (vertexMap.Length > 0)
            {
                Debug.Assert(vertexMap.Length == 1);
                AddMorphAnimations(mesh, vertexMap[0], elements, geometryBlock.MorphTargets);

                modelPrimitive.NeedsSkinnedRenderer = true;
            }

            return modelPrimitive;
        }

        /// <summary>
        /// Gets the vertices for a particular mesh component, optionally resolving vertex aliases if the component has
        /// them set.
        /// </summary>
        private static Vector3[] GetVerticesForMeshComponent(VertexElement vertices, MeshComponent component)
        {
            if (component.VertexAliases.Length == 0)
                return vertices.Data;
            // This component has aliased vertices, meaning it only uses a subset of the full vertices available.
            var usedVertices = new Vector3[component.VertexAliases.Length];
            Debug.Assert(component.VertexAliases.Length < ushort.MaxValue);
            for (ushort i = 0; i < component.VertexAliases.Length; i++)
            {
                usedVertices[i] = vertices.Data[component.VertexAliases[i]];
            }

            return usedVertices;
        }

        /// <summary>
        /// Same as `GetVerticesForMeshComponent` except for normal maps.
        /// </summary>
        private static Vector3[] GetNormalMapForMeshComponent(NormalElement normals, MeshComponent component)
        {
            if (component.NormalMapAliases.Length == 0)
                return normals.Data;
            var usedNormalMap = new Vector3[component.NormalMapAliases.Length];
            for (var i = 0; i < component.NormalMapAliases.Length; i++)
            {
                usedNormalMap[i] = normals.Data[component.NormalMapAliases[i]];
            }

            return usedNormalMap;
        }

        /// <summary>
        /// Same as `GetVerticesForMeshComponent` except for UV maps.
        /// </summary>
        private static Vector2[] GetUVMapForMeshComponent(UVMapElement uvMap, MeshComponent component)
        {
            if (component.UVMapAliases.Length == 0)
                return uvMap.Data;
            var usedUVMap = new Vector2[component.UVMapAliases.Length];
            for (var i = 0; i < component.UVMapAliases.Length; i++)
            {
                usedUVMap[i] = uvMap.Data[component.UVMapAliases[i]];
            }

            return usedUVMap;
        }

        private class MorphAnimation
        {
            internal GeometryDataContainerBlock.MorphTarget MorphTarget { get; }
            internal Vector3[] VertexPositionDeltas { get; }
            internal Vector3[] VertexNormalDeltas { get; }

            public MorphAnimation(GeometryDataContainerBlock.MorphTarget morphTarget, int meshVertexCount)
            {
                MorphTarget = morphTarget;
                VertexPositionDeltas = new Vector3[meshVertexCount];
                VertexNormalDeltas = new Vector3[meshVertexCount];
            }
        }

        private static void AddMorphAnimations(Mesh mesh, MorphVertexMapElement morphVertexMap,
            List<GeometryElement> elements, IReadOnlyList<GeometryDataContainerBlock.MorphTarget> morphTargets)
        {
            var vertexPosDeltas = elements.OfType<MorphVertexPositionDeltaElement>().ToArray();
            var vertexNormDeltas = elements.OfType<MorphNormalDeltaElement>().ToArray();

            // The maximum number of morph/blend animations we can have is the number of declared morph targets.
            var animations = new MorphAnimation[morphTargets.Count];

            for (var i = 0; i < morphVertexMap.Data.Length; i++)
            {
                var vertexMapData = morphVertexMap.Data[i];
                // This little uint32 requires a bit of explanation. It exists per vertex and it looks like this.
                //
                //       delta[0]
                //          v
                //    0x   aa bb cc dd <-- delta[3]
                //            ^   ^--------------- delta[2]
                //         delta[1]
                //
                // Where each byte `aa`, `bb`, `cc`, `dd` if non-zero represents both:
                //   * With its value, which morph target this morph animation belongs to.
                //   * With its position, which `MorphVertexPositionDeltaElement` out of a possible 4 in this mesh
                //     should affect its position.
                //
                // For example:
                //
                // 01_00_00_00 means morph data is in first vertexPosDeltas and this animation is in morph channel 1
                //
                // 01_04_00_00 means morph data is both in the first vertexPosDeltas and fourth vertexPosDeltas. The
                // first posDelta is part of morph channel 1 and the fourth posDelta is part of morph channel 4.
                if (vertexMapData == 0)
                {
                    continue;
                }

                // Iterate through each byte in the uint32.
                for (var byteIdx = 3; byteIdx >= 0; byteIdx--)
                {
                    var vertexMapByte = (vertexMapData >> 8 * (byteIdx)) & 0xFF;
                    if (vertexMapByte == 0)
                    {
                        continue;
                    }

                    // Leftmost (3rd) byte corresponds to the 0th delta map, rightmost (0th) byte corresponds to the
                    // 3rd delta map.
                    var deltaMapIdx = (3 - byteIdx);
                    Debug.Assert(deltaMapIdx < vertexPosDeltas.Length);
                    var morphTarget = morphTargets[(int)vertexMapByte];

                    if (animations[vertexMapByte] == null)
                    {
                        animations[vertexMapByte] = new MorphAnimation(morphTarget, mesh.vertexCount);
                    }

                    animations[vertexMapByte].VertexPositionDeltas[i] = vertexPosDeltas[deltaMapIdx].Data[i];
                    // Optionally the normal morph might be defined.
                    if (deltaMapIdx < vertexNormDeltas.Length)
                    {
                        animations[vertexMapByte].VertexNormalDeltas[i] = vertexNormDeltas[deltaMapIdx].Data[i];
                    }
                }
            }

            // Now we can actually attach the subset of morphs this mesh uses for unity. Only catch is unity requires
            // the frameWeight parameter to be increasing, so we just start at 99% and increment. Not sure if it should
            // be equally distributed yet.
            var weight = 99.9f;
            foreach (var animation in animations)
            {
                if (animation == null)
                {
                    continue;
                }
                mesh.AddBlendShapeFrame(animation.MorphTarget.channelName, weight, animation.VertexPositionDeltas,
                    animation.VertexNormalDeltas, null);
                weight += 0.00001f;
            }

            // We call recalculate tangents
            mesh.RecalculateTangents();
        }

        private static void AssignBones(ModelPrimitive primitive, IReadOnlyList<ushort> boneIndices,
            IReadOnlyList<GeometryDataContainerBlock.BindPose> bindPoses, BoneAssignmentsElement boneAssignments,
            IBoneWeightsElement boneWeights)
        {
            var bonesPerVertex = new byte[primitive.Mesh.vertexCount];
            // Allocate at least
            var weights = new List<BoneWeight1>(primitive.Mesh.vertexCount);
            // Need to keep track of this to allocate bind poses.
            var boneIds = new HashSet<int>();

            for (var i = 0; i < primitive.Mesh.vertexCount; i++)
            {
                // Total number of bones for this vertex.
                byte numBones = 0;
                // Iterate over the bytes in the bone assignment.
                for (var j = 0; j < 4; j++)
                {
                    var byteIdx = (3 - j);
                    var boneId = (boneAssignments.Data[i] >> (byteIdx * 8)) & 0xFF;
                    if (boneId == 255)
                    {
                        continue;
                    }
                    numBones++;
                    boneIds.Add((int)boneId);

                    var boneWeight = new BoneWeight1
                    {
                        boneIndex = (int)boneId,
                        weight = GetBoneWeight(boneWeights, i, j)
                    };
                    weights.Add(boneWeight);
                }
                bonesPerVertex[i] = numBones;
            }

            var bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);
            var weightsArray = new NativeArray<BoneWeight1>(weights.ToArray(), Allocator.Temp);
            primitive.Mesh.SetBoneWeights(bonesPerVertexArray, weightsArray);

            // Create a mapping of the local bone ids for the primitive to their global scenegraph ones and
            // set the bind poses for each bone.
            primitive.ScenegraphBoneIds = new ushort[boneIds.Count];
            primitive.BindPoses = new Matrix4x4[boneIds.Count];
            foreach (var boneId in boneIds)
            {
                // Look up the real/scenegraph boneId.
                var mappedBoneId = boneIndices[boneId];
                primitive.ScenegraphBoneIds[boneId] = mappedBoneId;
                // Get the bone bind pose.
                var bindPose = bindPoses[mappedBoneId];
                primitive.BindPoses[boneId] = Matrix4x4.TRS(bindPose.Position, bindPose.Rotation, Vector3.one);
            }
        }

        /// <summary>
        /// Grabs the bone weight for the given vertex index from boneWeights for the nth bone assignment number.
        /// </summary>
        private static float GetBoneWeight(IBoneWeightsElement boneWeights, int vertexIdx, int boneNumber)
        {
            return boneWeights switch
            {
                BoneWeightsForSingleBonesElement element => element.Data[vertexIdx],
                BoneWeightsForTwoBonesElement element => element.Data[vertexIdx][boneNumber],
                BoneWeightsForThreeBonesElement element => element.Data[vertexIdx][boneNumber],
                _ => throw new ArgumentException($"Unknown boneWeights type: {boneWeights}")
            };
        }
    }
}