using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    public class UITextElement : UIElement
    {
        public override UIComponent Instantiate(Transform parent)
        {
            var uiComponent = base.Instantiate(parent);
            var text = uiComponent.gameObject.AddComponent<Text>();
            text.text = Caption;
            return uiComponent;
        }
    }
}
