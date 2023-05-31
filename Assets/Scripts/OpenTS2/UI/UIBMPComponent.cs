using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using OpenTS2.Engine;

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
        public bool OwnTexture = false;
        public RawImage RawImageComponent => GetComponent<RawImage>();
        private void OnDestroy()
        {
            if (OwnTexture)
            {
                if (RawImageComponent != null)
                    (RawImageComponent.texture as Texture2D).Free();
            }
        }
    }
}
