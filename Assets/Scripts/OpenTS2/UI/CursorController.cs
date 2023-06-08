using OpenTS2.Content;
using OpenTS2.Files;
using System;
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

        /// <summary>
        /// Current Hardware Cursor being rendered.
        /// </summary>
        public static CursorType Cursor
        {
            get
            {
                return _cursor;
            }
            set
            {
                _cursor = value;
#if !UNITY_EDITOR
                SetCursorInternal();
#endif
            }
        }

        private static CursorType _cursor = CursorType.Default;

        /// <summary>
        /// Registers a cursor relative to TSData, in the newest Product it can be found in.
        /// </summary>
        /// <param name="type">Cursor type to register.</param>
        /// <param name="filename">Cursor path relative to TSData.</param>
        private void RegisterCursorRelativePath(CursorType type, string filename)
        {
            var absolutePath = Filesystem.GetLatestFilePath(Path.Combine("Res/UI/Cursors",filename));
            if (absolutePath != null)
                RegisterCursorAbsolutePath(type, absolutePath);
        }

        /// <summary>
        /// Registers a cursor by its absolute path.
        /// </summary>
        /// <param name="type">Cursor type to register.</param>
        /// <param name="filename">Path to cursor file.</param>
        private void RegisterCursorAbsolutePath(CursorType type, string filename)
        {
            if (File.Exists(filename))
            {
                HardwareCursors.InitializeCursor((int)type, filename);
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
            RegisterCursorRelativePath(CursorType.Default, "arrow_8.cur");
            RegisterCursorRelativePath(CursorType.Hourglass, "Hourglass_8.ani");
            // Sledgehammer was added in Pets.
            RegisterCursorRelativePath(CursorType.Sledgehammer, "Sledgehammer_8.cur");
        }

#if !UNITY_EDITOR
        static void SetCursorInternal()
        {
            if (Application.isFocused)
                HardwareCursors.SetCurrentCursor((int)Cursor);
        }

        void Update()
        {
            SetCursorInternal();

        }
#endif
    }
}
