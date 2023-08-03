using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using OpenTS2.Diagnostic;

namespace OpenTS2.Rendering
{
    public static class Batching
    {
        /// <summary>
        /// Final list of meshes to be batched into 1.
        /// </summary>
        struct PreparedBatch
        {
            public int VertexAmount;
            public Material Material;
            public List<BatchCandidate> Candidates;

            public PreparedBatch(Material material)
            {
                Material = material;
                VertexAmount = 0;
                Candidates = new List<BatchCandidate>();
            }
        }

        /// <summary>
        /// Candidate mesh for batching.
        /// </summary>
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

        /// <summary>
        /// Stores results of batching.
        /// </summary>
        public class BatchResult
        {
            /// <summary>
            /// Parent GameObject holding all batched meshes.
            /// </summary>
            public GameObject BatchedObjectsParent;
            /// <summary>
            /// Original renderers that were disabled by batching.
            /// </summary>
            public List<MeshRenderer> DisabledRenderers = new List<MeshRenderer>();

            /// <summary>
            /// Re-enables all original renderers that were disabled by batching.
            /// </summary>
            public void RestoreVisibility()
            {
                foreach (var renderer in DisabledRenderers)
                    renderer.enabled = true;
            }
        }

        // Might not play nice with some hardware, maybe adjust conditionally.
        [ConsoleProperty("ots2_batchingVertexLimit")]
        public static int DefaultVertexLimit = 10000000;
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

        // If there's anything we don't want getting batched, add it here.
        static bool CanBatch(MeshRenderer renderer, MeshFilter filter)
        {
            if (!CanBatchShader(renderer.sharedMaterial.shader))
                return false;
            return true;
        }

        /// <summary>
        /// Merge meshes sharing the same materials into fewer optimized meshes with all transforms applied.
        /// </summary>
        /// <param name="parent">Parent transform that contains all meshes.</param>
        /// <param name="flipFaces">Whether to flip faces. Set this to true if the meshes are using TS2's coordinate system.</param>
        /// <returns>Parent GameObject containing all batched meshes.</returns>
        public static BatchResult Batch(Transform parent, bool flipFaces = false, int vertexLimit = 0)
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
                if (!CanBatch(renderer, filter))
                    continue;
                var candidate = new BatchCandidate(material, mesh, renderer, filter.transform);
                if (!candidatesByMaterial.TryGetValue(material, out List<BatchCandidate> candidates))
                {
                    candidates = new List<BatchCandidate>();
                    candidatesByMaterial[material] = candidates;
                }
                candidates.Add(candidate);
            }

            if (vertexLimit <= 0)
                vertexLimit = DefaultVertexLimit;
            var preparedBatches = new List<PreparedBatch>();

            foreach(var mat in candidatesByMaterial)
            {
                var material = mat.Key;
                var candidateList = mat.Value;
                var currentBatch = new PreparedBatch(material);
                foreach(var candidate in candidateList)
                {
                    currentBatch.VertexAmount += candidate.Mesh.vertexCount;
                    currentBatch.Candidates.Add(candidate);
                    if (currentBatch.VertexAmount > vertexLimit)
                    {
                        if (currentBatch.Candidates.Count > 1)
                            preparedBatches.Add(currentBatch);
                        currentBatch = new PreparedBatch(material);
                    }
                }
                if (currentBatch.Candidates.Count > 1)
                    preparedBatches.Add(currentBatch);
            }

            var finalGameObject = new GameObject("Batched Objects");
            result.BatchedObjectsParent = finalGameObject;

            foreach (var batch in preparedBatches)
            {
                var mesh = new Mesh();
                if (batch.VertexAmount >= UInt16.MaxValue)
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                var batchGameObject = new GameObject($"Batched: {batch.Material.name}", typeof(BatchedComponent), typeof(MeshRenderer), typeof(MeshFilter));

                batchGameObject.transform.SetParent(finalGameObject.transform);

                var batchedComponent = batchGameObject.GetComponent<BatchedComponent>();
                var renderer = batchGameObject.GetComponent<MeshRenderer>();
                var filter = batchGameObject.GetComponent<MeshFilter>();

                batchedComponent.BatchedMesh = mesh;
                renderer.sharedMaterial = batch.Material;
                filter.sharedMesh = mesh;

                var index = 0;
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var triangles = new List<int>();
                var uvs = new List<Vector2>();

                foreach(var candidate in batch.Candidates)
                {
                    candidate.Renderer.enabled = false;
                    result.DisabledRenderers.Add(candidate.Renderer);

                    var c_vertices = new List<Vector3>();
                    var c_normals = new List<Vector3>();
                    var c_uvs = new List<Vector2>();
                    var c_triangles = candidate.Mesh.GetTriangles(0);

                    candidate.Mesh.GetVertices(c_vertices);
                    candidate.Mesh.GetNormals(c_normals);
                    candidate.Mesh.GetUVs(0, c_uvs);

                    for (var i = 0; i < c_vertices.Count; i++)
                    {
                        c_vertices[i] = candidate.Transform.TransformPoint(c_vertices[i]);
                    }
                    for (var i = 0; i < c_normals.Count; i++)
                    {
                        c_normals[i] = candidate.Transform.TransformDirection(c_normals[i]);
                    }
                    for (var i = 0; i < c_triangles.Length; i++)
                    {
                        c_triangles[i] += index;
                    }

                    index += c_vertices.Count;
                    vertices.AddRange(c_vertices);
                    normals.AddRange(c_normals);
                    triangles.AddRange(c_triangles);
                    uvs.AddRange(c_uvs);
                }

                if (flipFaces)
                    triangles.Reverse();

                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetTriangles(triangles, 0);
                mesh.SetUVs(0, uvs);
                mesh.RecalculateBounds();
                mesh.Optimize();
            }
            return result;
        }
    }
}