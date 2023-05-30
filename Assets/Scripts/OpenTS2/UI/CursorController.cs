using OpenTS2.Content;
using OpenTS2.Files;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenTS2.UI {
    public class CursorController : MonoBehaviour
    {
        public static CursorController Singleton => s_singleton;
        static CursorController s_singleton = null;
        public enum CursorType
        {
            Default,
            Hourglass,
            Sledgehammer
        }

        public static CursorType Cursor = CursorType.Default;

        private void RegisterCursorFromLatestProduct(CursorType type, string filename)
        {
            var epManager = EPManager.Get();
            RegisterCursor(type, epManager.GetLatestProduct(), filename);
        }

        private void RegisterCursor(CursorType type, ProductFlags product, string filename)
        {
            var cursorLocation = Path.Combine(Filesystem.GetDataPathForProduct(product), "Res/UI/Cursors", filename);
            if (File.Exists(cursorLocation))
            {
                HardwareCursors.InitializeCursor((int)type, cursorLocation);
            }
        }

        private void Awake()
        {
            if (s_singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            s_singleton = this;
            DontDestroyOnLoad(gameObject);
            RegisterCursors();
        }

        void RegisterCursors()
        {
            RegisterCursor(CursorType.Default, ProductFlags.BaseGame, "arrow_8.cur");
            RegisterCursor(CursorType.Hourglass, ProductFlags.BaseGame, "Hourglass_8.ani");
            // Sledgehammer was added in Pets, IIRC, so Pets and later all come with the cursor in the UI files.
            RegisterCursorFromLatestProduct(CursorType.Sledgehammer, "Sledgehammer_8.cur");
        }

#if !UNITY_EDITOR
        void Update()
        {
            if (Application.isFocused)
                HardwareCursors.SetCurrentCursor((int)Cursor);
        }
#endif
    }
}
