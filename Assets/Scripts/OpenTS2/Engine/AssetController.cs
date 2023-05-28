using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine
{
    /// <summary>
    /// Singleton component that loads Unity assets such as the default Font.
    /// </summary>
    public class AssetController : MonoBehaviour
    {
        public static AssetController Singleton => s_singleton;
        public static Font DefaultFont => s_singleton._defaultFont;
        static AssetController s_singleton = null;
        [SerializeField]
        private Font _defaultFont;
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
    }
}
