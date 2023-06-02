using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.UI
{
    /// <summary>
    /// Creates UI Elements.
    /// </summary>
    public static class UIElementFactory
    {
        /// <summary>
        /// Creates an instance of the appropriate UI Element class, given properties.
        /// </summary>
        /// <param name="properties">UI Properties</param>
        /// <returns></returns>
        public static UIElement CreateFromProperties(UIProperties properties)
        {
            var cls = properties.GetProperty("clsid");
            UIElement element = null;
            switch(cls)
            {
                case "GZWinGen":
                    element = new UIBMPElement();
                    break;
                case "GZWinBMP":
                    element = new UIBMPElement();
                    break;
                case "GZWinFlatRect":
                    element = new UIFlatRectElement();
                    break;
                case "GZWinTextEdit":
                    element = new UITextEditElement();
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
