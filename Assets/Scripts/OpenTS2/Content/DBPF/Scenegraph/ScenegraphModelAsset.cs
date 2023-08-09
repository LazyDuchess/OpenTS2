using System;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block.GeometryData;
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
        public Dictionary<String, Mesh> Primitives { get; } = new Dictionary<String, Mesh>();

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
                prim.Value.Free();
            }

            StaticBoundMesh.Free();
        }

        private Mesh InitializeMeshFromPrimitive(GeometryDataContainerBlock geometryBlock, MeshPrimitive primitive)
        {
            var mesh = new Mesh
            {
                name = geometryBlock.Resource.ResourceName + "_" + primitive.Name
            };

            var meshComponent = geometryBlock.GetMeshComponentForPrimitive(primitive);
            var elements = geometryBlock.GetGeometryElementsForMeshComponent(meshComponent);
            var vertices = elements.OfType<VertexElement>().Single();

            mesh.SetVertices(GetVerticesForMeshComponent(vertices, meshComponent));
            mesh.SetTriangles(primitive.Faces, 0);

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

            // Add morph animations if present.
            var vertexMap = elements.OfType<MorphVertexMapElement>().ToArray();
            if (vertexMap.Length > 0)
            {
                Debug.Assert(vertexMap.Length == 1);
                AddMorphAnimations(mesh, vertexMap[0], elements, geometryBlock.MorphTargets);
            }

            return mesh;
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
            var weight = 99f;
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
    }
}