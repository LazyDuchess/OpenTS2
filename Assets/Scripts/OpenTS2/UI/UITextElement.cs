using OpenTS2.Engine;
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
    /// UI Text.
    /// </summary>
    public class UITextElement : UIElement
    {
        protected override Type UIComponentType => typeof(UITextComponent);
        public TextAnchor Alignment = TextAnchor.MiddleCenter;
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
                        Alignment = TextAnchor.UpperLeft;
                        break;
                    case "centertop":
                        Alignment = TextAnchor.UpperCenter;
                        break;
                    case "righttop":
                        Alignment = TextAnchor.UpperRight;
                        break;
                    case "leftcenter":
                        Alignment = TextAnchor.MiddleLeft;
                        break;
                    case "center":
                        Alignment = TextAnchor.MiddleCenter;
                        break;
                    case "rightcenter":
                        Alignment = TextAnchor.MiddleRight;
                        break;
                    case "leftbottom":
                        Alignment = TextAnchor.LowerLeft;
                        break;
                    case "centerbottom":
                        Alignment = TextAnchor.LowerCenter;
                        break;
                    case "rightbottom":
                        Alignment = TextAnchor.LowerRight;
                        break;
                }
            }
        }
        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent);
            var textGameObject = new GameObject("Text");
            textGameObject.transform.SetParent(uiComponent.transform);
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
            rect.offsetMax = new Vector2(1f, 1f);
            return uiComponent;
        }
    }
}
