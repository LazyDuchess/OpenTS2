using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Scenes
{
    public class NeighborhoodWater : MonoBehaviour
    {
        private void Start()
        {
            
            transform.position += NeighborhoodManager.CurrentNeighborhood.Terrain.SeaLevel * Vector3.up;
            if (CameraReflection.Instance != null)
            {
                var meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.sharedMaterial.SetTexture("_Reflection", CameraReflection.Instance.ReflectionTexture);
            }
        }
    }
}
