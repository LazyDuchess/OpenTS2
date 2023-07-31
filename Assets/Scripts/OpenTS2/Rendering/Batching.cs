using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace OpenTS2.Rendering
{
    public static class Batching
    {
        struct BatchCandidate
        {
            public Mesh Mesh;
            public Transform Transform;
            public int SubMesh;
            public BatchCandidate(Mesh mesh, Transform transform, int submesh)
            {
                Mesh = mesh;
                Transform = transform;
                SubMesh = submesh;
            }
        }

        /// <summary>
        /// Merge meshes sharing the same materials into fewer optimized meshes with all transforms applied.
        /// </summary>
        /// <param name="parent">Parent transform that contains all meshes.</param>
        /// <param name="flipFaces">Whether to flip faces. Set this to true if the meshes are using TS2's coordinate system.</param>
        /// <returns>Parent GameObject containing all batched meshes.</returns>
        public static GameObject Batch(Transform parent, bool flipFaces = false)
        {
            // TODO - Maybe make this function multithreaded.
            var candidatesByMaterial = new Dictionary<Material, List<BatchCandidate>>();
            var renderers = parent.GetComponentsInChildren<MeshRenderer>();
            foreach (var element in renderers)
            {
                var filter = element.GetComponent<MeshFilter>();
                if (filter == null)
                    continue;
                var materials = element.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var mat = materials[i];
                    if (filter.sharedMesh.subMeshCount <= i)
                        continue;
                    if (!candidatesByMaterial.TryGetValue(mat, out List<BatchCandidate> candidates))
                    {
                        candidates = new List<BatchCandidate>();
                        candidatesByMaterial[mat] = candidates;
                    }
                    candidates.Add(new BatchCandidate(filter.sharedMesh, filter.transform, i));
                }
            }
            var filteredMaterials = candidatesByMaterial;
            var finalGameObject = new GameObject("Batched Objects");
            foreach (var material in filteredMaterials)
            {
                var mesh = new Mesh();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                var batchCandidates = material.Value;
                var currentIndex = 0;
                var verts = new List<Vector3>();
                var norms = new List<Vector3>();
                var tris = new List<int>();
                var uvs = new List<Vector2>();

                var candidateGameObject = new GameObject($"Batch: {material.Key.name}", typeof(BatchedComponent));
                var batchedComponent = candidateGameObject.GetComponent<BatchedComponent>();
                batchedComponent.BatchedMesh = mesh;
                candidateGameObject.transform.SetParent(finalGameObject.transform);
                var renderer = candidateGameObject.AddComponent<MeshRenderer>();
                var filter = candidateGameObject.AddComponent<MeshFilter>();
                renderer.sharedMaterial = material.Key;
                filter.sharedMesh = mesh;

                foreach (var candidate in batchCandidates)
                {
                    var vertices = new List<Vector3>();
                    var normals = new List<Vector3>();
                    var uv = new List<Vector2>();
                    candidate.Mesh.GetVertices(vertices);
                    var triangles = candidate.Mesh.GetTriangles(candidate.SubMesh);
                    candidate.Mesh.GetNormals(normals);
                    candidate.Mesh.GetUVs(0, uv);
                    for (var i = 0; i < vertices.Count; i++)
                    {
                        vertices[i] = candidate.Transform.TransformPoint(vertices[i]);
                    }
                    for(var i=0;i<normals.Count;i++)
                    {
                        normals[i] = candidate.Transform.TransformDirection(normals[i]);
                    }
                    for (var i = 0; i < triangles.Length; i++)
                    {
                        triangles[i] += currentIndex;
                    }
                    if (flipFaces)
                        triangles = triangles.Reverse().ToArray();
                    currentIndex += vertices.Count;
                    verts.AddRange(vertices);
                    norms.AddRange(normals);
                    tris.AddRange(triangles);
                    uvs.AddRange(uv);
                }
                mesh.SetVertices(verts);
                mesh.SetNormals(norms);
                mesh.SetTriangles(tris, 0);
                mesh.SetUVs(0, uvs);
                mesh.RecalculateBounds();
                mesh.Optimize();
            }
            return finalGameObject;
        }
    }
}