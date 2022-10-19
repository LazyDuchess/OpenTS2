using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OpenTS2.Engine.Tests
{
    public class UICursorTestComponent : MonoBehaviour
    {
        public Canvas parentCanvas;

        // Update is called once per frame
        void Update()
        {
            Vector2 movePos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition, parentCanvas.worldCamera,
                out movePos);

            transform.position = parentCanvas.transform.TransformPoint(movePos);
            transform.Rotate(Vector3.forward * 80f * Time.deltaTime);
        }
    }
}