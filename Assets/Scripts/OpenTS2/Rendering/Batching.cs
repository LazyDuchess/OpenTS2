using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace OpenTS2.Rendering
{
    public static class Batching
    {
        struct NonBatchedCandidate
        {
            public Mesh Mesh;
            public Material Material;
            public Transform Transform;
            public NonBatchedCandidate(Mesh mesh, Material material, Transform transform)
            {
                Mesh = mesh;
                Material = material;
                Transform = transform;
            }
        }

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

        static Dictionary<Shader, bool> s_shadersCantBeBatched = new Dictionary<Shader, bool>();

        /// <summary>
        /// Prevents specific shaders from getting batched.
        /// </summary>
        /// <param name="shaders">Names of shaders to ignore when batching meshes together.</param>
        public static void MarkShadersNoBatching(params string[] shaders)
        {
            foreach(var shaderName in shaders)
            {
                var shader = Shader.Find(shaderName);
                MarkShadersNoBatching(shader);
            }
        }

        /// <summary>
        /// Prevents specific shaders from getting batched.
        /// </summary>
        /// <param name="shaders">Shaders to ignore when batching meshes together.</param>
        public static void MarkShadersNoBatching(params Shader[] shaders)
        {
            foreach(var shader in shaders)
                s_shadersCantBeBatched[shader] = true;
        }

        static bool CanBatchShader(Shader shader)
        {
            if (!s_shadersCantBeBatched.ContainsKey(shader))
                return true;
            return false;
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
            var nonBatchedCandidates = new List<NonBatchedCandidate>();
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
                    if (!CanBatchShader(mat.shader))
                    {
                        nonBatchedCandidates.Add(new NonBatchedCandidate(filter.sharedMesh, mat, filter.transform));
                        continue;
                    }
                    if (!candidatesByMaterial.TryGetValue(mat, out List<BatchCandidate> candidates))
                    {
                        candidates = new List<BatchCandidate>();
                        candidatesByMaterial[mat] = candidates;
                    }
                    candidates.Add(new BatchCandidate(filter.sharedMesh, filter.transform, i));
                }
            }
            var finalGameObject = new GameObject("Batched Objects");
            foreach (var material in candidatesByMaterial)
            {
                if (material.Value.Count == 1)
                {
                    MakeNonBatched(material.Value[0].Transform, material.Value[0].Mesh, material.Key);
                    continue;
                }
                var mesh = new Mesh();
                var vertAmount = 0;
                foreach(var candidate in material.Value)
                {
                    vertAmount += candidate.Mesh.vertexCount;
                }
                if (vertAmount >= UInt16.MaxValue)
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                var batchCandidates = material.Value;
                var currentIndex = 0;
                var verts = new List<Vector3>();
                var norms = new List<Vector3>();
                var tris = new List<int>();
                var uvs = new List<Vector2>();

                var candidateGameObject = new GameObject($"Batched: {material.Key.name}", typeof(BatchedComponent), typeof(MeshRenderer), typeof(MeshFilter));
                candidateGameObject.transform.SetParent(finalGameObject.transform);
                var batchedComponent = candidateGameObject.GetComponent<BatchedComponent>();
                var renderer = candidateGameObject.GetComponent<MeshRenderer>();
                var filter = candidateGameObject.GetComponent<MeshFilter>();
                batchedComponent.BatchedMesh = mesh;
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
            foreach(var nonBatched in nonBatchedCandidates)
            {
                MakeNonBatched(nonBatched.Transform, nonBatched.Mesh, nonBatched.Material);
            }
            return finalGameObject;

            void MakeNonBatched(Transform transform, Mesh mesh, Material material)
            {
                var candidateGameObject = new GameObject($"Non-Batched: {material.name}", typeof(MeshRenderer), typeof(MeshFilter));
                candidateGameObject.transform.SetParent(finalGameObject.transform);
                candidateGameObject.transform.position = transform.position;
                candidateGameObject.transform.rotation = transform.rotation;
                candidateGameObject.transform.localScale = transform.lossyScale;
                var renderer = candidateGameObject.GetComponent<MeshRenderer>();
                var filter = candidateGameObject.GetComponent<MeshFilter>();
                renderer.sharedMaterial = material;
                filter.sharedMesh = mesh;
            }
        }
    }
}