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
                default:
                    element = new UIElement();
                    break;
            }
            element.ParseProperties(properties);
            return element;
        }
    }
}
