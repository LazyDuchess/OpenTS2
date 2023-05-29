using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    /// <summary>
    /// Represents a UI Script.
    /// </summary>
    public class UILayout : AbstractAsset
    {
        /// <summary>
        /// Returns root elements.
        /// </summary>
        public List<UIElement> Elements = new List<UIElement>();
        /// <summary>
        /// Returns all elements recursively.
        /// </summary>
        public List<UIElement> AllElements
        {
            get
            {
                var elementList = new List<UIElement>();
                foreach(var element in Elements)
                {
                    elementList.AddRange(GetElements(element));
                }
                return elementList;
                List<UIElement> GetElements(UIElement element)
                {
                    var elementList = new List<UIElement>();
                    foreach (var child in element.Children)
                    {
                        elementList.Add(child);
                        elementList.AddRange(GetElements(child));
                    }
                    return elementList;
                }
            }
        }

        public UIComponent[] Instantiate(Transform parent)
        {
            var elementArray = new UIComponent[Elements.Count];
            for (var i = 0; i < Elements.Count; i++)
            {
                var uiElement = Elements[i].Instantiate(parent);
                elementArray[i] = uiElement;
            }
            return elementArray;
        }
    }
}
