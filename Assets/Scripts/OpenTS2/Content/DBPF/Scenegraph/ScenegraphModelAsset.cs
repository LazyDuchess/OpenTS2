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

        private Mesh InitializeMeshFromPrimitive(GeometryDataContainerBlock geometryBlock, MeshPrimitive primitive)
        {
            var mesh = new Mesh
            {
                name = geometryBlock.Resource.ResourceName + "_" + primitive.Name
            };

            var elements = geometryBlock.GetGeometryElementsForPrimitive(primitive);
            var vertices = elements.OfType<VertexElement>().Single();

            mesh.SetVertices(vertices.Data);
            mesh.SetTriangles(primitive.Faces, 0);

            foreach (var geometryElement in elements)
            {
                switch (geometryElement)
                {
                    case VertexElement _:
                        // Handled before this loop.
                        break;
                    case NormalElement normals:
                        mesh.SetNormals(normals.Data);
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
                        mesh.SetUVs(0, uvMap.Data);
                        break;
                    default:
                        Debug.LogWarning($"Unknown geometry element type: {geometryElement.GetType()}");
                        break;
                }
            }

            return mesh;
        }

        public override void FreeUnmanagedResources()
        {
            foreach(var prim in Primitives)
            {
                prim.Value.Free();
            }
            StaticBoundMesh.Free();
        }
    }
}