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
    public class UIBMPComponent : UIComponent
    {
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
