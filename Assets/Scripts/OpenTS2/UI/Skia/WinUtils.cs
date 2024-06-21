using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Skia
{
    public unsafe static class WinUtils
    {
        [DllImport("user32.dll")]
        private static extern uint GetCaretBlinkTime();

        public static float GetCaretBlinkTimer()
        {
            return GetCaretBlinkTime() * 0.001f;
        }
    }
}
