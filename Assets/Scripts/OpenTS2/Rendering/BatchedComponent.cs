using OpenTS2.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Rendering
{
    public class BatchedComponent : MonoBehaviour
    {
        public Mesh BatchedMesh;
        private void OnDestroy()
        {
            if (BatchedMesh != null)
                BatchedMesh.Free();
        }
    }
}
