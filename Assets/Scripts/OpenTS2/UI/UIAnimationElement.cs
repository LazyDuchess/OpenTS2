using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    /// <summary>
    /// Animated UI Element.
    /// </summary>
    public class UIAnimationElement : UIBMPElement
    {
        protected override Type UIComponentType => typeof(UIAnimationComponent);
        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent) as UIAnimationComponent;
            uiComponent.SetTexture(uiComponent.RawImageComponent.texture as Texture2D, (int)Area.width);
            return uiComponent;
        }
    }
}
