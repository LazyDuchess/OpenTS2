using OpenTS2.Components;
using System.Collections.Generic;
using UnityEngine;


namespace OpenTS2.Scenes.Lot
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class LotFloorPatternComponent : AssetReferenceComponent
    {
        public Mesh Mesh;
        public Material Material;

        private List<Vector3> _vertexBuilder = new List<Vector3>();
        private List<Vector2> _vertexUvBuilder = new List<Vector2>();
        private List<int> _indexBuilder = new List<int>();

        public void Initialize(Material material)
        {
            Mesh = new Mesh();
            Material = material;

            GetComponent<MeshFilter>().sharedMesh = Mesh;

            var meshRenderer = GetComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = material;
        }

        public int GetVertexIndex()
        {
            return _vertexBuilder.Count;
        }

        public void AddVertex(Vector3 vert, Vector2 uv)
        {
            _vertexBuilder.Add(vert);
            _vertexUvBuilder.Add(uv);
        }

        public void AddVertices(Vector3[] vertices, Vector2[] uvs)
        {
            _vertexBuilder.AddRange(vertices);
            _vertexUvBuilder.AddRange(uvs);
        }

        public void AddTriangle(int baseVertex, int a, int b, int c)
        {
            _indexBuilder.Add(baseVertex + a);
            _indexBuilder.Add(baseVertex + b);
            _indexBuilder.Add(baseVertex + c);
        }

        public void AddIndex(int index)
        {
            _indexBuilder.Add(index);
        }

        public void AddWindingRect(int baseVert)
        {
            _indexBuilder.Add(baseVert);
            _indexBuilder.Add(baseVert + 1);
            _indexBuilder.Add(baseVert + 4);

            _indexBuilder.Add(baseVert + 1);
            _indexBuilder.Add(baseVert + 2);
            _indexBuilder.Add(baseVert + 4);

            _indexBuilder.Add(baseVert + 2);
            _indexBuilder.Add(baseVert + 3);
            _indexBuilder.Add(baseVert + 4);

            _indexBuilder.Add(baseVert + 3);
            _indexBuilder.Add(baseVert);
            _indexBuilder.Add(baseVert + 4);
        }

        public void Clear()
        {
            _vertexBuilder.Clear();
            _vertexUvBuilder.Clear();
            _indexBuilder.Clear();
        }

        public void Commit()
        {
            Mesh.SetVertices(_vertexBuilder);
            Mesh.SetUVs(0, _vertexUvBuilder);
            Mesh.SetIndices(_indexBuilder, MeshTopology.Triangles, 0);

            Mesh.RecalculateNormals();
            Mesh.RecalculateTangents();
            Mesh.RecalculateBounds();
        }
    }
}