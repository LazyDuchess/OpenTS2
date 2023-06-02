using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
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
    /// UI Button.
    /// </summary>
    public class UIButtonElement : UIElement
    {
        public ResourceKey Image = default;
        protected override Type UIComponentType => typeof(UIButtonComponent);
        public override void ParseProperties(UIProperties properties)
        {
            base.ParseProperties(properties);
            Image = properties.GetImageKeyProperty("image");
        }
        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent) as UIButtonComponent;
            var rawImage = uiComponent.gameObject.AddComponent<RawImage>();
            if (IgnoreMouse)
                rawImage.raycastTarget = false;
            var contentProvider = ContentProvider.Get();
            var imageAsset = contentProvider.GetAsset<TextureAsset>(Image);
            if (imageAsset != null)
                uiComponent.SetTexture(imageAsset);
            return uiComponent;
        }
    }
}
