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
            Stretch,
            Center,
            BottomLeft,
            TopLeft,
            BottomRight,
            TopRight
        }
        public UIElement Element;
        public RectTransform RectTransformComponent => GetComponent<RectTransform>();
        public UIComponent[] Children => transform.GetComponentsInChildren<UIComponent>(true);

        public Vector2 GetCenter()
        {
            var pos = RectTransformComponent.position;
            var size = RectTransformComponent.sizeDelta;
            return new Vector2(pos.x + size.x / 2f, pos.y - size.y / 2f);
        }

        public void SetPositionCentered(Vector2 position)
        {
            var size = RectTransformComponent.sizeDelta;
            RectTransformComponent.position = position - (new Vector2(size.x / 2f, -size.y / 2f));
        }

        public void SetAnchor(AnchorType anchor)
        {
            switch(anchor)
            {
                case AnchorType.Stretch:
                    RectTransformComponent.anchorMin = new Vector2(0f, 0f);
                    RectTransformComponent.anchorMax = new Vector2(1f, 1f);
                    RectTransformComponent.anchoredPosition = Vector2.zero;
                    RectTransformComponent.sizeDelta = Vector2.zero;
                    return;

                case AnchorType.Center:
                    RectTransformComponent.anchorMin = new Vector2(0.5f, 0.5f);
                    RectTransformComponent.anchorMax = new Vector2(0.5f, 0.5f);
                    RectTransformComponent.anchoredPosition = new Vector2(-RectTransformComponent.sizeDelta.x / 2f, RectTransformComponent.sizeDelta.y / 2f);
                    return;

                case AnchorType.BottomLeft:
                    RectTransformComponent.anchorMin = new Vector2(0f, 0f);
                    RectTransformComponent.anchorMax = new Vector2(0f, 0f);
                    RectTransformComponent.anchoredPosition = new Vector2(0f, RectTransformComponent.sizeDelta.y);
                    return;

                case AnchorType.TopLeft:
                    RectTransformComponent.anchorMin = new Vector2(0f, 1f);
                    RectTransformComponent.anchorMax = new Vector2(0f, 1f);
                    RectTransformComponent.anchoredPosition = Vector2.zero;
                    return;

                case AnchorType.BottomRight:
                    RectTransformComponent.anchorMin = new Vector2(1f, 0f);
                    RectTransformComponent.anchorMax = new Vector2(1f, 0f);
                    RectTransformComponent.anchoredPosition = new Vector2(RectTransformComponent.sizeDelta.x, RectTransformComponent.sizeDelta.y);
                    return;

                case AnchorType.TopRight:
                    RectTransformComponent.anchorMin = new Vector2(1f, 1f);
                    RectTransformComponent.anchorMax = new Vector2(1f, 1f);
                    RectTransformComponent.anchoredPosition = new Vector2(RectTransformComponent.sizeDelta.x, 0f);
                    return;
            }
        }

        public T GetChildByID<T>(uint id, bool includeInactive = true) where T : UIComponent
        {
            var components = GetChildrenByID<T>(id, includeInactive);
            if (components.Count <= 0)
                return null;
            return components[0];
        }

        public UIComponent GetChildByID(uint id, bool includeInactive = true)
        {
            var components = GetChildrenByID(id, includeInactive);
            if (components.Count <= 0)
                return null;
            return components[0];
        }

        public List<T> GetChildrenByID<T>(uint id, bool includeInactive = true) where T : UIComponent
        {
            var components = transform.GetComponentsInChildren<T>(includeInactive);
            return components.Where((component) => component.Element.ID == id).ToList();
        }

        public List<UIComponent> GetChildrenByID(uint id, bool includeInactive = true)
        {
            var components = transform.GetComponentsInChildren<UIComponent>(includeInactive);
            return components.Where((component) => component.Element.ID == id).ToList();
        }
    }
}
