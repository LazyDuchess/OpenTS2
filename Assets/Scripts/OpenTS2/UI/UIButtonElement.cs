using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    public class UIButtonElement : UIBMPElement
    {
        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent) as UIBMPComponent;
            var rawImage = uiComponent.RawImageComponent;
            if (rawImage.texture != null)
            {
                rawImage.texture = UIUtils.SplitTextureHorizontalSequence(rawImage.texture as Texture2D, (int)Area.width)[1];
                rawImage.SetNativeSize();
            }
            return uiComponent;
        }
    }
}
