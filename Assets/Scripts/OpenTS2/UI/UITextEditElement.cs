﻿using OpenTS2.UI.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    public class UITextEditElement : UITextElement
    {
        public bool SingleLine = true;
        protected override Type UIComponentType => typeof(UITextEditComponent);
        public override void ParseProperties(UIProperties properties)
        {
            base.ParseProperties(properties);
            SingleLine = properties.GetBoolProperty("singleline");
        }
        public override UIComponent Instantiate(Transform parent)
        {
            var component = base.Instantiate(parent) as UITextEditComponent;
            var inputField = component.gameObject.AddComponent<SkiaInputField>();
            inputField.Label = component.TextComponent;

            var caret = new GameObject("Caret");
            caret.transform.parent = component.transform;
            var rawImage = caret.AddComponent<RawImage>();
            rawImage.color = ForeColor;
            rawImage.raycastTarget = false;

            inputField.Caret = caret.GetComponent<RectTransform>();
            inputField.Caret.anchorMin = new Vector2(0f, 1f);
            inputField.Caret.anchorMax = new Vector2(0f, 1f);
            inputField.Caret.pivot = new Vector2(0.5f, 1f);
            inputField.OnTextEdited += component.FireTextEdited;
            /*
            if (SingleLine)
                inputField.lineType = InputField.LineType.MultiLineSubmit;
            else
                inputField.lineType = InputField.LineType.MultiLineNewline;
            inputField.onEndEdit.AddListener(delegate { component.CheckTextEdited(); });
            component.gameObject.AddComponent<TextFieldBehaviour>();*/
            return component;
        }
    }
}
