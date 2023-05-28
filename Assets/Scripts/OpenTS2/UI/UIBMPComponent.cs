using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    public class UIBMPComponent : UIComponent
    {
        public RawImage RawImageComponent => GetComponent<RawImage>();
    }
}
