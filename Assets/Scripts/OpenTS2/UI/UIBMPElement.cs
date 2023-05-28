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
    public class UIBMPElement : UIElement
    {
        public ResourceKey Image = default;
        protected override Type UIComponentType => typeof(UIBMPComponent);
        public override void ParseProperties(UIProperties properties)
        {
            base.ParseProperties(properties);
            Image = properties.GetImageKeyProperty("image");
        }

        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent);
            var contentProvider = ContentProvider.Get();
            var imageAsset = contentProvider.GetAsset<TextureAsset>(Image);
            var rawImage = uiComponent.gameObject.AddComponent<RawImage>();
            if (imageAsset != null)
            {
                rawImage.texture = imageAsset.Texture;
            }
            else
                rawImage.color = new Color(0f, 0f, 0f, 0f);
            return uiComponent;
        }
    }
}
