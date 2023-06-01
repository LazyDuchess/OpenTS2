using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    /// <summary>
    /// Generic UI Element.
    /// </summary>
    public class UIElement
    {
        public bool IgnoreMouse = false;
        public bool Visible = true;
        public uint ID = 0x0;
        public Rect Area = Rect.zero;
        public Color32 FillColor = Color.black;
        public string Caption = "";
        public List<UIElement> Children = new List<UIElement>();
        public UIElement Parent = null;
        protected virtual Type UIComponentType => typeof(UIComponent);

        public virtual void ParseProperties(UIProperties properties)
        {
            ID = properties.GetHexProperty("id");
            Area = properties.GetRectProperty("area");
            FillColor = properties.GetColorProperty("fillcolor");
            Caption = properties.GetProperty("caption");
            Visible = properties.GetBoolProperty("winflag_visible");
            IgnoreMouse = properties.GetBoolProperty("winflag_ignoremouse");
        }

        public virtual UIComponent Instantiate(Transform parent)
        {
            var element = new GameObject(ToString());
            element.SetActive(Visible);
            element.transform.SetParent(parent);
            var rectTransform = element.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(Area.x, -Area.y);
            rectTransform.sizeDelta = new Vector2(Area.width, Area.height);

            var uiComponent = element.AddComponent(UIComponentType);
            (uiComponent as UIComponent).Element = this;

            foreach (var child in Children)
            {
                child.Instantiate(element.transform);
            }
            return (UIComponent)uiComponent;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Caption))
                return $"0x{ID.ToString("X8")}";
            return $"{Caption}, ID: 0x{ID.ToString("X8")}";
        }
    }
}
