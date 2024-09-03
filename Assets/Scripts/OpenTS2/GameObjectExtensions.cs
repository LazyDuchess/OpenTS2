using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2
{
    public static class GameObjectExtensions
    {
        public static void SetLayerHierarchy(this GameObject go, int layer)
        {
            var transforms = go.GetComponentsInChildren<Transform>();
            foreach(var transform in transforms)
            {
                transform.gameObject.layer = layer;
            }
        }
    }
}
