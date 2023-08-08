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
            foreach(var prim in Primitives)
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

                    // Handled below in animation section.
                    case MorphNormalDeltaElement _:
                    case MorphVertexPositionDeltaElement _:
                    // These might just be the indices of the vertices that are affected by the vertex delta and
                    // normal delta.
                    case MorphNormalIndicesElement _:
                    case MorphVertexPositionIndicesElement _:
                        break;

                    default:
                        Debug.LogWarning($"Unknown geometry element type: {geometryElement.GetType()}");
                        break;
                }
            }


            // Add blend animations.
            var deltaVertices = elements.OfType<MorphVertexPositionDeltaElement>().ToArray();
            var deltaNormals = elements.OfType<MorphNormalDeltaElement>().ToArray();

            for (var i = 0; i < deltaVertices.Length; i++)
            {
                mesh.AddBlendShapeFrame($"blend-{i}", 100.0f, deltaVertices[i].Data, deltaNormals[i].Data, null);
            }
            mesh.RecalculateTangents();

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

    }
}