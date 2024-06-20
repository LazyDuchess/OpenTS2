using OpenTS2.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using OpenTS2.UI.Skia;

namespace OpenTS2.UI
{
    /// <summary>
    /// UI Text.
    /// </summary>
    public class UITextElement : UIElement
    {
        protected override Type UIComponentType => typeof(UITextComponent);
        public HorizontalAlign HorizontalAlign = HorizontalAlign.Left;
        public VerticalAlign VerticalAlign = VerticalAlign.Top;
        public Color32 ForeColor = Color.black;

        public override void ParseProperties(UIProperties properties)
        {
            base.ParseProperties(properties);
            if (properties.HasProperty("captionres"))
            {
                var captionRes = properties.GetStringSetProperty("captionres");
                Caption = captionRes.GetLocalizedString();
            }
            if (properties.GetProperty("clsid") == "GZWinTextEdit")
                ForeColor = properties.GetColorProperty("caretcolor");
            else
                ForeColor = properties.GetColorProperty("forecolor");
            var align = properties.GetProperty("align");
            if (align != null)
            {
                switch(align)
                {
                    case "lefttop":
                        HorizontalAlign = HorizontalAlign.Left;
                        VerticalAlign = VerticalAlign.Top;
                        break;
                    case "centertop":
                        HorizontalAlign = HorizontalAlign.Center;
                        VerticalAlign = VerticalAlign.Top;
                        break;
                    case "righttop":
                        HorizontalAlign = HorizontalAlign.Right;
                        VerticalAlign = VerticalAlign.Top;
                        break;
                    case "leftcenter":
                        HorizontalAlign = HorizontalAlign.Left;
                        VerticalAlign = VerticalAlign.Middle;
                        break;
                    case "center":
                        HorizontalAlign = HorizontalAlign.Center;
                        VerticalAlign = VerticalAlign.Middle;
                        break;
                    case "rightcenter":
                        HorizontalAlign = HorizontalAlign.Right;
                        VerticalAlign = VerticalAlign.Middle;
                        break;
                    case "leftbottom":
                        HorizontalAlign = HorizontalAlign.Left;
                        VerticalAlign = VerticalAlign.Bottom;
                        break;
                    case "centerbottom":
                        HorizontalAlign = HorizontalAlign.Center;
                        VerticalAlign = VerticalAlign.Bottom;
                        break;
                    case "rightbottom":
                        HorizontalAlign = HorizontalAlign.Right;
                        VerticalAlign = VerticalAlign.Bottom;
                        break;
                }
            }
        }
        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent);
            var textGameObject = new GameObject("Text");
            textGameObject.transform.SetParent(uiComponent.transform);
            var text = textGameObject.AddComponent<SkiaLabel>();
            text.FontColor = ForeColor;
            text.Text = Caption;
            text.FontSize = 14;
            text.HorizontalAlign = HorizontalAlign;
            text.VerticalAlign = VerticalAlign;
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(1f, 1f);
            rect.anchorMin = Vector2.zero;

            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(1f, 1f);
            /*
            var text = textGameObject.AddComponent<Text>();
            text.color = ForeColor;
            text.text = Caption;
            text.font = AssetController.DefaultFont;
            text.fontSize = 14;
            text.alignment = Alignment;
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(1f, 1f);
            rect.anchorMin = Vector2.zero;

            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(1f, 1f);*/
            return uiComponent;
        }
    }
}
