using OpenTS2.Common;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Layouts
{
    /// <summary>
    /// Represents a physical instance of a UI Layout.
    /// </summary>
    public abstract class UILayoutInstance
    {
        public UIComponent[] Components;
        protected static Transform MainCanvas => UIManager.MainCanvas.transform;
        protected abstract ResourceKey UILayoutResourceKey { get; }
        public UILayoutInstance(Transform parent)
        {
            var contentProvider = ContentProvider.Get();
            var layout = contentProvider.GetAsset<UILayout>(UILayoutResourceKey);
            Components = layout.Instantiate(parent);
        }
    }
}
