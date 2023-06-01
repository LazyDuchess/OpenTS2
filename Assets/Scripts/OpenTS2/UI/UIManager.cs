using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    public static class UIManager
    {
        public static Canvas MainCanvas = null;
        public static bool AnyMouseButtonHeld
        {
            get
            {
                return Input.GetMouseButton(0) || Input.GetMouseButton(1);
            }
        }
    }
}
