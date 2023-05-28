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
        public override void ParseProperties(UIProperties properties)
        {
            base.ParseProperties(properties);
            Image = properties.GetImageKeyProperty("image");
        }

        public override GameObject Instantiate(Transform parent)
        {
            var gameObject = base.Instantiate(parent);
            var rawImage = gameObject.GetComponent<RawImage>();
            var contentProvider = ContentProvider.Get();
            var imageAsset = contentProvider.GetAsset<TextureAsset>(Image);
            if (imageAsset != null)
            {
                rawImage.texture = imageAsset.Texture;
                rawImage.SetNativeSize();
                rawImage.color = Color.white;
            }
            return gameObject;
        }
    }
}
