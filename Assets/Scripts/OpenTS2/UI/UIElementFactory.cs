using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.UI
{
    public static class UIElementFactory
    {
        public static UIElement CreateFromProperties(UIProperties properties)
        {
            var cls = properties.GetProperty("clsid");
            UIElement element = null;
            switch(cls)
            {
                case "GZWinBMP":
                    element = new UIBMPElement();
                    break;
                case "GZWinFlatRect":
                    element = new UIFlatRectElement();
                    break;
                case "GZWinText":
                    element = new UITextElement();
                    break;
                case "GZWinBtn":
                    element = new UIButtonElement();
                    break;
                    // Animated elements use this.
                case "0x4d9ccdb1":
                    element = new UIAnimationElement();
                    break;
                default:
                    element = new UIElement();
                    break;
            }
            element.ParseProperties(properties);
            return element;
        }
    }
}
