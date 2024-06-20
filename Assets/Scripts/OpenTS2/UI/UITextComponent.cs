using OpenTS2.UI.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    public class UITextComponent : UIComponent
    {
        public virtual string Text
        {
            get
            {
                return TextComponent.Text;
            }
            set
            {
                TextComponent.Text = value;
            }
        }
        public SkiaLabel TextComponent => transform.GetComponentInChildren<SkiaLabel>();
    }
}
