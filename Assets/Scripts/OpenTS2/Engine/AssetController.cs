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
        public static AssetController Instance { get; private set; }
        public static Font DefaultFont => Instance._defaultFont;
        [SerializeField]
        private Font _defaultFont;
        private void Awake()
        {
            Instance = this;
        }
    }
}
