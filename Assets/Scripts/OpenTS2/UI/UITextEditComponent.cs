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
        private string _originalText;
        public Action<string> OnTextEdited;
        public InputField Input => GetComponent<InputField>();
        public override string Text
        {
            get
            {
                return Input.text;
            }
            set
            {
                Input.text = value;
                _originalText = value;
            }
        }

        public void CheckTextEdited()
        {
            if (Text != _originalText)
            {
                Debug.Log($"TEXT EDITED! ({_originalText}) => ({Text})");
                OnTextEdited?.Invoke(Text);
                _originalText = Text;
            }
        }
    }
}
