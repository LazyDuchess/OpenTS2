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
            public Matrix4x4 Transform;
            public int SubMesh;
            public BatchCandidate(Mesh mesh, Matrix4x4 transform, int submesh)
            {
                Mesh = mesh;
                Transform = transform;
                SubMesh = submesh;
            }
        }
        public static GameObject Batch(GameObject gameObject)
        {
            return null;
            var candidatesByMaterial = new Dictionary<Material, List<BatchCandidate>>();
            var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var element in renderers)
            {
                var filter = element.GetComponent<MeshFilter>();
                if (filter == null)
                    continue;
                var materials = element.materials;
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
                    candidates.Add(new BatchCandidate(filter.sharedMesh, filter.transform.localToWorldMatrix, i));
                }
            }
            //var filteredMaterials = candidatesByMaterial.Where((x) => x.Value.Count > 1).ToDictionary(x => x.Key, x => x.Value);
            var filteredMaterials = candidatesByMaterial;
            var materialToMesh = new Dictionary<Material, Mesh>();
            var finalGameObject = new GameObject("Batched Objects");
            foreach (var material in filteredMaterials)
            {
                var mesh = new Mesh();
                var batchCandidates = material.Value;
                var currentIndex = 0;
                var verts = new List<Vector3>();
                var norms = new List<Vector3>();
                var tris = new List<int>();
                var uvs = new List<Vector2>();

                var candidateGameObject = new GameObject($"Batch: {material.Key.name}");
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
                        vertices[i] = candidate.Transform * vertices[i];
                    }
                    for (var i = 0; i < triangles.Length; i++)
                    {
                        triangles[i] += currentIndex;
                    }
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
            }
            return finalGameObject;
        }
    }
}