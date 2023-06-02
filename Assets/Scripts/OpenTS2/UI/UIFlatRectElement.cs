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
    /// Colored rectangle.
    /// </summary>
    public class UIFlatRectElement : UIElement
    {
        public enum Styles
        {
            fill,
            nofill
        }
        private Styles _style = Styles.fill;
        protected override Type UIComponentType => typeof(UIBMPComponent);
        public override void ParseProperties(UIProperties properties)
        {
            base.ParseProperties(properties);
            var fill = properties.GetProperty("style");
            if (fill != null)
            {
                switch(fill)
                {
                    case "nofill":
                        _style = Styles.nofill;
                        break;
                }
            }
        }
        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent);
            var rawImage = uiComponent.gameObject.AddComponent<RawImage>();
            rawImage.color = Color.clear;
            if (_style != Styles.nofill)
                rawImage.color = FillColor;
            return uiComponent;
        }
    }
}
