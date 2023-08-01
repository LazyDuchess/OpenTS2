using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace OpenTS2.Rendering
{
    public static class Batching
    {
        struct BatchCandidate
        {
            public Material Material;
            public Mesh Mesh;
            public MeshRenderer Renderer;
            public Transform Transform;
            public BatchCandidate(Material material, Mesh mesh, MeshRenderer renderer, Transform transform)
            {
                Material = material;
                Mesh = mesh;
                Transform = transform;
                Renderer = renderer;
            }
        }

        public class BatchResult
        {
            public GameObject BatchedObjectsParent;
            public List<MeshRenderer> DisabledRenderers = new List<MeshRenderer>();

            public void RestoreVisibility()
            {
                foreach (var renderer in DisabledRenderers)
                    renderer.enabled = true;
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
        public static BatchResult Batch(Transform parent, bool flipFaces = false)
        {
            var result = new BatchResult();
            // TODO - Maybe make this function multithreaded.
            var candidatesByMaterial = new Dictionary<Material, List<BatchCandidate>>();
            var renderers = parent.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter == null)
                    continue;
                if (filter.sharedMesh == null)
                    continue;
                if (renderer.sharedMaterial == null)
                    continue;
                if (filter.sharedMesh.subMeshCount > 1)
                    continue;
                var mesh = filter.sharedMesh;
                var material = renderer.sharedMaterial;
                if (!CanBatchShader(material.shader))
                    continue;
                var candidate = new BatchCandidate(material, mesh, renderer, filter.transform);
                if (!candidatesByMaterial.TryGetValue(material, out List<BatchCandidate> candidates))
                {
                    candidates = new List<BatchCandidate>();
                    candidatesByMaterial[material] = candidates;
                }
                candidates.Add(candidate);
            }
            var finalGameObject = new GameObject("Batched Objects");
            result.BatchedObjectsParent = finalGameObject;
            foreach (var material in candidatesByMaterial)
            {
                if (material.Value.Count == 1)
                    continue;
                var mesh = new Mesh();
                var vertAmount = 0;
                foreach (var candidate in material.Value)
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
                    result.DisabledRenderers.Add(candidate.Renderer);
                    candidate.Renderer.enabled = false;
                    var vertices = new List<Vector3>();
                    var normals = new List<Vector3>();
                    var uv = new List<Vector2>();
                    candidate.Mesh.GetVertices(vertices);
                    var triangles = candidate.Mesh.GetTriangles(0);
                    candidate.Mesh.GetNormals(normals);
                    candidate.Mesh.GetUVs(0, uv);
                    for (var i = 0; i < vertices.Count; i++)
                    {
                        vertices[i] = candidate.Transform.TransformPoint(vertices[i]);
                    }
                    for (var i = 0; i < normals.Count; i++)
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
            return result;
        }
    }
}