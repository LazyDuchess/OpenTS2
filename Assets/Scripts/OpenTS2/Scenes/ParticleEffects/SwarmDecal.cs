using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Diagnostic;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Scenes.ParticleEffects
{
    public class SwarmDecal : MonoBehaviour
    {
        private static int Location = Shader.PropertyToID("_Location");
        private static int Rotation = Shader.PropertyToID("_Rotation");
        private ScenegraphTextureAsset _textureAsset;
        private Vector3 _position;
        private Vector3 _rotation;
        private Material _material;
        private Mesh _decalMesh;

        private void Start()
        {
            _position = transform.position;
            _rotation = transform.rotation.eulerAngles;
            Initialize();
        }

        public void SetDecal(DecalEffect effect)
        {
            _textureAsset = ContentManager.Instance.GetAsset<ScenegraphTextureAsset>(new ResourceKey($"{effect.TextureName}_txtr", GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_TXTR));
        }

        public void Initialize()
        {
            if (_material != null)
                _material.Free();
            var nhoodTerrain = NeighborhoodTerrain.Instance;
            if (nhoodTerrain == null)
                return;
            var terrainMeshFilter = nhoodTerrain.GetComponent<MeshFilter>();

            var meshFilter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();

            BuildDecalMesh(terrainMeshFilter.sharedMesh);
            meshFilter.sharedMesh = _decalMesh;

            _material = new Material(Shader.Find("OpenTS2/BakedDecal"));
            _material.mainTexture = _textureAsset.GetSelectedImageAsUnityTexture();

            meshRenderer.sharedMaterial = _material;
            transform.SetParent(null);
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private void BuildDecalMesh(Mesh terrainMesh)
        {
            _decalMesh = new Mesh();
            var vertices = terrainMesh.vertices;
            var uvs = new Vector2[terrainMesh.vertices.Length];

            var oTriangles = terrainMesh.triangles;
            var triangles = new List<int>();

            var radius = 80f;
            var radiusHalf = radius * 0.5f;
            var decalPositionMatrix = Matrix4x4.Translate(new Vector3(-_position.x + radiusHalf, 0f, -_position.z + radiusHalf));
            var decalRotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0f, -_rotation.y, 0f));

            for(var i = 0; i < vertices.Length; i++)
            {
                var v = decalPositionMatrix.MultiplyPoint(vertices[i]);
                v /= radius;
                v.x -= 0.5f;
                v.z -= 0.5f;
                v = decalRotationMatrix.MultiplyPoint(v);
                v.x += 0.5f;
                v.z += 0.5f;

                uvs[i] = new Vector2(v.x, v.z);
            }
            
            for (var i = 0; i < oTriangles.Length; i += 3)
            {
                var v1i = oTriangles[i];
                var v2i = oTriangles[i + 1];
                var v3i = oTriangles[i + 2];

                var v1 = uvs[v1i];
                var v2 = uvs[v2i];
                var v3 = uvs[v3i];

                if ((v1.x < 1f && v1.y < 1f && v1.x > 0f && v1.y > 0f) ||
                    (v2.x < 1f && v2.y < 1f && v2.x > 0f && v2.y > 0f) ||
                    (v3.x < 1f && v3.y < 1f && v3.x > 0f && v3.y > 0f))
                {
                    triangles.Add(v1i);
                    triangles.Add(v2i);
                    triangles.Add(v3i);
                }
            }

            _decalMesh.SetVertices(vertices);
            _decalMesh.SetUVs(0, uvs);
            _decalMesh.SetTriangles(triangles, 0);
        }

        private void OnDestroy()
        {
            if (_material != null)
                _material.Free();
            if (_decalMesh != null)
                _decalMesh.Free();
        }
    }
}
