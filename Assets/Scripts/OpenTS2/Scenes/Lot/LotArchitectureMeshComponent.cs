using OpenTS2.Components;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes.Lot
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class LotArchitectureMeshComponent : AssetReferenceComponent
    {
        public Mesh Mesh;
        public Material Material;

        private List<Vector3> _vertexBuilder = new List<Vector3>();
        private List<Vector2> _vertexUvBuilder = new List<Vector2>();
        private List<int> _indexBuilder = new List<int>();

        private MeshRenderer _renderer;
        private bool _enableShadows = true;
        private bool _visible = true;

        public void Initialize(Material material)
        {
            Mesh = new Mesh();
            Material = material;

            GetComponent<MeshFilter>().sharedMesh = Mesh;

            _renderer = GetComponent<MeshRenderer>();

            _renderer.sharedMaterial = material;
        }

        public void EnableExtraUV()
        {

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

        public void EnableShadows(bool enable)
        {
            _enableShadows = enable;

            UpdateVisibility(true);
        }

        private void UpdateVisibility(bool shadowChange)
        {
            if (_enableShadows)
            {
                if (shadowChange)
                {
                    _renderer.enabled = true;
                }

                _renderer.shadowCastingMode = _visible ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            else
            {
                if (shadowChange)
                {
                    _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                _renderer.enabled = _visible;
            }
        }

        public void SetVisible(bool visible)
        {
            if (visible != _visible)
            {
                _visible = visible;

                UpdateVisibility(false);
            }
        }

        public bool Commit()
        {
            if (_vertexBuilder.Count > 65536 && Mesh.indexFormat == UnityEngine.Rendering.IndexFormat.UInt16)
            {
                Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            Mesh.SetVertices(_vertexBuilder);
            Mesh.SetUVs(0, _vertexUvBuilder);
            Mesh.SetIndices(_indexBuilder, MeshTopology.Triangles, 0);

            Mesh.RecalculateNormals();
            Mesh.RecalculateTangents();
            Mesh.RecalculateBounds();

            return _vertexBuilder.Count > 0 || _indexBuilder.Count > 0;
        }
    }
}