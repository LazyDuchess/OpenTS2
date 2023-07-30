using OpenTS2.Content;
using OpenTS2.Rendering;
using OpenTS2.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Diagnostic
{
    public class DebugTerrainEditor : MonoBehaviour
    {
        public float BrushSize = 20f;
        public float BrushForce = 5f;
        public float SmoothForce = 0.1f;
        public bool UpdateColliderRealtime = false;
        public bool UpdateNormalsRealtime = false;
        bool _usingTool = false;
        int _terrainLayerMask = (1 << 3);
        private void Start()
        {
            NeighborhoodTerrain.Instance.GetComponent<MeshFilter>().sharedMesh.MarkDynamic();
        }
        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.F1))
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    _usingTool = true;
                    DoRaise(BrushForce);
                    return;
                }
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    _usingTool = true;
                    DoRaise(-BrushForce);
                    return;
                }
            }
            if (Input.GetKey(KeyCode.F2))
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    _usingTool = true;
                    DoSmooth(SmoothForce);
                    return;
                }
            }
            if (_usingTool)
            {
                _usingTool = false;
                var terrainInstance = NeighborhoodTerrain.Instance;
                if (terrainInstance != null)
                {
                    var meshFilter = terrainInstance.GetComponent<MeshFilter>();
                    var collider = terrainInstance.GetComponent<MeshCollider>();
                    
                    if (meshFilter != null && collider != null)
                    {
                        if (!UpdateNormalsRealtime)
                            meshFilter.sharedMesh.RecalculateNormals();
                        if (!UpdateColliderRealtime)
                            collider.sharedMesh = meshFilter.sharedMesh;
                        var terrainAsset = NeighborhoodManager.CurrentNeighborhood.Terrain;
                        terrainAsset.FromMesh(meshFilter.sharedMesh);
                        terrainAsset.Compressed = true;
                        terrainAsset.Save();
                    }
                }
            }
        }

        void DoSmooth(float force)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 2000f, _terrainLayerMask))
            {
                var terrain = hitInfo.collider.GetComponent<NeighborhoodTerrain>();
                if (terrain == null)
                    return;
                var terrainMesh = terrain.GetComponent<MeshFilter>().sharedMesh;
                var vertices = terrainMesh.vertices;
                for (var i = 0; i < vertices.Length; i++)
                {
                    var hit = hitInfo.point;
                    var smoothTarget = hit.y;
                    //hit.y = vertices[i].y;
                    var dist = Vector3.Distance(vertices[i], hit);
                    dist /= BrushSize;
                    dist = Mathf.Min(1, dist);
                    dist = -(dist * dist) + 1;

                    var smoothDelta = smoothTarget - vertices[i].y;

                    vertices[i].y += smoothDelta * force * dist;
                }
                terrainMesh.vertices = vertices;
                LightmapManager.RenderShadowMap();
                if (UpdateNormalsRealtime)
                    terrainMesh.RecalculateNormals();
                if (UpdateColliderRealtime)
                {
                    (hitInfo.collider as MeshCollider).sharedMesh = terrainMesh;
                }
            }
        }

        void DoRaise(float force)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 2000f, _terrainLayerMask))
            {
                var terrain = hitInfo.collider.GetComponent<NeighborhoodTerrain>();
                if (terrain == null)
                    return;
                var terrainMesh = terrain.GetComponent<MeshFilter>().sharedMesh;
                var vertices = terrainMesh.vertices;
                for(var i=0;i<vertices.Length;i++)
                {
                    var hit = hitInfo.point;
                    var dist = Vector3.Distance(vertices[i], hit);
                    dist /= BrushSize;
                    dist = Mathf.Min(1, dist);
                    dist = -(dist * dist) + 1;
                    vertices[i].y += force * dist;
                }
                terrainMesh.vertices = vertices;
                LightmapManager.RenderShadowMap();
                terrainMesh.RecalculateBounds();
                if (UpdateNormalsRealtime)
                    terrainMesh.RecalculateNormals();
                if (UpdateColliderRealtime)
                {
                    (hitInfo.collider as MeshCollider).sharedMesh = terrainMesh;
                }
            }
        }
    }
}
