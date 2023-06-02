using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace OpenTS2.UI
{
    // Source: https://stackoverflow.com/a/41487400/20826912
    /// <summary>
    /// Hack to disable whole text highlight behavior when selecting InputFields.
    /// </summary>
    public class TextFieldBehaviour : MonoBehaviour, ISelectHandler
    {
        private InputField inputField;
        private bool isCaretPositionReset = false;

        void Start()
        {
            inputField = gameObject.GetComponent<InputField>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            StartCoroutine(disableHighlight());
        }

        IEnumerator disableHighlight()
        {
            //Get original selection color
            Color originalTextColor = inputField.selectionColor;
            //Remove alpha
            originalTextColor.a = 0f;

            //Apply new selection color without alpha
            inputField.selectionColor = originalTextColor;

            //Wait one Frame(MUST DO THIS!)
            yield return null;

            //Change the caret pos to the end of the text
            inputField.MoveTextEnd(false);

            //Return alpha
            originalTextColor.a = 1f;

            //Apply new selection color with alpha
            inputField.selectionColor = originalTextColor;
        }
    }
}
