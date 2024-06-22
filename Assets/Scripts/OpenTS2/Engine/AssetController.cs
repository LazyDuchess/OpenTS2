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
        public static AssetController Singleton { get; private set; }
        public static Font DefaultFont => Singleton._defaultFont;
        [SerializeField]
        private Font _defaultFont;
        private void Awake()
        {
            Singleton = this;
        }
    }
}
