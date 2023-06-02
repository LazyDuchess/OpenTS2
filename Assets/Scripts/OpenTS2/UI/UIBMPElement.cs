using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    /// <summary>
    /// Static image UI Element.
    /// </summary>
    public class UIBMPElement : UIElement
    {
        public ResourceKey Image = default;
        public bool EdgeImage = false;
        protected override Type UIComponentType => typeof(UIBMPComponent);
        public override void ParseProperties(UIProperties properties)
        {
            base.ParseProperties(properties);
            Image = properties.GetImageKeyProperty("image");
            EdgeImage = properties.GetBoolProperty("edgeimage");
            // TODO: Is this right?
            if (properties.GetProperty("clsid") == "GZWinGen")
                EdgeImage = true;
        }

        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent) as UIBMPComponent;
            var contentProvider = ContentProvider.Get();
            var imageAsset = contentProvider.GetAsset<TextureAsset>(Image);
            var rawImage = uiComponent.gameObject.AddComponent<RawImage>();
            if (IgnoreMouse)
                rawImage.raycastTarget = false;
            if (imageAsset != null)
            {
                if (EdgeImage)
                {
                    var edgeTexture = UIUtils.MakeEdgeImage(imageAsset.Texture, (int)Area.width, (int)Area.height);
                    var edgeAsset = new TextureAsset(edgeTexture);
                    uiComponent.SetTexture(edgeAsset);
                }
                else
                    uiComponent.SetTexture(imageAsset);
            }
            else
                rawImage.color = new Color(0f, 0f, 0f, 0f);
            return uiComponent;
        }
    }
}
