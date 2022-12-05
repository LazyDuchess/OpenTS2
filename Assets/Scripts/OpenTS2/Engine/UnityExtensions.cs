using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine
{
    internal static class UnityExtensions
    {
        public static void Free(this Texture2D obj)
        {
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
