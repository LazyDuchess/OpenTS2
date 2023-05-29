using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    /// <summary>
    /// UI Button.
    /// </summary>
    public class UIButtonElement : UIBMPElement
    {
        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent) as UIBMPComponent;
            var rawImage = uiComponent.RawImageComponent;
            if (rawImage.texture != null)
            {
                rawImage.texture = UIUtils.SplitTextureHorizontalSequence(rawImage.texture as Texture2D, rawImage.texture.width / 4)[0];
            }
            return uiComponent;
        }
    }
}
