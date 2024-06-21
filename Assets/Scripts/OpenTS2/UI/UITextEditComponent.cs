using OpenTS2.UI.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    public class UITextEditComponent : UITextComponent
    {
        public Action<string> OnTextEdited;
        public SkiaInputField Input => GetComponent<SkiaInputField>();
        public override string Text
        {
            get
            {
                return Input.Text;
            }
            set
            {
                Input.Text = value;
            }
        }

        public void FireTextEdited()
        {
            OnTextEdited?.Invoke(Input.Text);
        }
    }
}
