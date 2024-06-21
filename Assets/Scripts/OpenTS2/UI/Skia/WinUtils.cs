using System.Runtime.InteropServices;

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
