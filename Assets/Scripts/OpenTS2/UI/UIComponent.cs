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
    /// Generic UI Element Component.
    /// </summary>
    public class UIComponent : MonoBehaviour
    {
        public enum AnchorType
        {
            Center
        }
        public UIElement Element;
        public RectTransform RectTransformComponent => GetComponent<RectTransform>();

        public void SetAnchor(AnchorType anchor)
        {
            switch(anchor)
            {
                case AnchorType.Center:
                    RectTransformComponent.anchorMin = new Vector2(0.5f, 0.5f);
                    RectTransformComponent.anchorMax = new Vector2(0.5f, 0.5f);
                    transform.localPosition = new Vector3(-RectTransformComponent.sizeDelta.x / 2f, RectTransformComponent.sizeDelta.y / 2f, 0f);
                    return;
            }
        }

        public UIComponent GetChildByID(uint id)
        {
            var components = transform.GetComponentsInChildren<UIComponent>(true);
            foreach(var element in components)
            {
                if (element.Element.ID == id)
                    return element;
            }
            return null;
        }
    }
}
