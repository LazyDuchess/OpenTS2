using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine
{
    /// <summary>
    /// Manages clean up of resources that have been garbage collected.
    /// </summary>
    public class MemoryController : MonoBehaviour
    {
        private static Action MarkedForRemoval;

        public static void MarkForRemoval(Action action)
        {
            lock (MarkedForRemoval)
                MarkedForRemoval += action;
        }
        private void Update()
        {
            if (MarkedForRemoval == null)
                return;
            lock (MarkedForRemoval)
            {
                MarkedForRemoval.Invoke();
                MarkedForRemoval = null;
            }
        }
    }
}
