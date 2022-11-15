using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OpenTS2.Engine.Tests
{
    public class UICursorTestComponent : MonoBehaviour
    {
        public Canvas ParentCanvas;

        // Update is called once per frame
        void Update()
        {

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                ParentCanvas.transform as RectTransform,
                Input.mousePosition, ParentCanvas.worldCamera,
                out Vector2 movePos);

            transform.position = ParentCanvas.transform.TransformPoint(movePos);
            transform.Rotate(80f * Time.deltaTime * Vector3.forward);
        }
    }
}