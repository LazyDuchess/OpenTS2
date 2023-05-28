using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    public class UIComponent : MonoBehaviour
    {
        public UIElement Element;
        public RectTransform RectTransformComponent => GetComponent<RectTransform>();

        public UIComponent GetChildByID(uint id)
        {
            var components = transform.GetComponentsInChildren<UIComponent>();
            foreach(var element in components)
            {
                if (element.Element.ID == id)
                    return element;
            }
            return null;
        }
    }
}
