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
    public class UIElement
    {
        public uint ID = 0x0;
        public Rect Area = Rect.zero;
        public Color32 FillColor = Color.black;
        public string Caption = "";
        public List<UIElement> Children = new List<UIElement>();
        public ResourceKey Image = default;
        public UIElement Parent = null;

        public void Instantiate(Transform parent)
        {
            var contentProvider = ContentProvider.Get();
            var tex = contentProvider.GetAsset<TextureAsset>(Image);
            var element = new GameObject(ToString());
            element.transform.SetParent(parent);
            var rawImage = element.AddComponent<RawImage>();
            if (tex != null)
                rawImage.texture = tex.Texture;
            else
                rawImage.color = FillColor;
            var rectTransform = element.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(Area.x, -Area.y);

            //rectTransform.position = new Vector2(Area.x, Area.y);
            //rectTransform.sizeDelta = new Vector2(Area.width, Area.height);
            rawImage.SetNativeSize();
            if (tex == null)
                rectTransform.sizeDelta = new Vector2(Area.width, Area.height);
            foreach (var child in Children)
            {
                //if (!child.Image.Equals(default(ResourceKey)))
                    child.Instantiate(element.transform);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Caption))
                return $"0x{ID.ToString("X8")}";
            return $"{Caption}, ID: 0x{ID.ToString("X8")}";
        }
    }
}
