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
        public static MemoryController Singleton => s_singleton;
        static MemoryController s_singleton = null;

        private static Action MarkedForRemoval;

        private void Awake()
        {
            if (s_singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            s_singleton = this;
            DontDestroyOnLoad(gameObject);
        }

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
