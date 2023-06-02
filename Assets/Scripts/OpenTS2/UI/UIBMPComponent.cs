using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using OpenTS2.Engine;
using OpenTS2.Content.DBPF;

namespace OpenTS2.UI
{
    /// <summary>
    /// Static Image UI Component.
    /// </summary>
    public class UIBMPComponent : UIComponent
    {
        public Texture2D Texture
        {
            get
            {
                return RawImageComponent.texture as Texture2D;
            }
            set
            {
                RawImageComponent.texture = value;
            }
        }
        public Color Color
        {
            get
            {
                return RawImageComponent.color;
            }

            set
            {
                RawImageComponent.color = value;
            }
        }
        private TextureAsset _textureReference;
        public RawImage RawImageComponent => GetComponent<RawImage>();

        public void SetNativeSize()
        {
            RawImageComponent.SetNativeSize();
        }
        public void SetTexture(TextureAsset asset)
        {
            _textureReference = asset;
            RawImageComponent.texture = asset.Texture;
        }
    }
}
