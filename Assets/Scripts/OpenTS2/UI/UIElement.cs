using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    public class UIElement
    {
        public uint ID = 0x0;
        public Rect Area = Rect.zero;
        public Color32 FillColor = Color.black;
        public string Caption = "";
        public List<UIElement> Children = new List<UIElement>();
        public UIElement Parent = null;
    }
}
