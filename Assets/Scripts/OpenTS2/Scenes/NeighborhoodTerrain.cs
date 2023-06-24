using OpenTS2.Content;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public class NeighborhoodTerrain : MonoBehaviour
    {
        public float RayDistance = 2000f;
        public float RayBias = 1f;
        public float BlurOffset = 10f;
        public Transform Sun;
        public bool Reload = false;
        // Start is called before the first frame update
        void Start()
        {
            SetTerrainMesh();
        }

        private void Update()
        {
            if (Reload)
            {
                var meshFilter = GetComponent<MeshFilter>();
                MakeVertexColors(meshFilter.sharedMesh);
                Reload = false;
            }
        }

        void SetTerrainMesh()
        {
            var terrainAsset = NeighborhoodManager.CurrentNeighborhood.Terrain;
            var terrainMesh = terrainAsset.MakeMesh();
            var meshFilter = GetComponent<MeshFilter>();
            var meshCollider = GetComponent<MeshCollider>();
            meshFilter.sharedMesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
            MakeVertexColors(terrainMesh);
        }

        void MakeVertexColors(Mesh terrainMesh)
        {
            var vertices = terrainMesh.vertices;
            var normals = terrainMesh.normals;
            var colors = new Color[vertices.Length];
            for(var i=0;i<vertices.Length;i++)
            {
                var vertex = vertices[i];
                var normal = normals[i];
                var color = Color.black;
                var shadowAccumulator = 0f;
                for (var j=-1;j<2;j++)
                {
                    for(var n=-1;n<2;n++)
                    {
                        var offX = BlurOffset * j;
                        var offZ = BlurOffset * n;
                        var rayTarget = vertex + (normal * RayBias) + offX * Vector3.right + offZ * Vector3.forward;
                        var ray = new Ray(rayTarget, -Sun.forward);
                        if (Physics.Raycast(ray, RayDistance))
                            shadowAccumulator += 1f;
                    }
                }
                shadowAccumulator /= 9f;
                color.r = shadowAccumulator;
                colors[i] = color;
            }
            terrainMesh.colors = colors;
        }
    }
}
