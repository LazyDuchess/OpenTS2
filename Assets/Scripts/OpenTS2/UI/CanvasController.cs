using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasController : MonoBehaviour
    {
        private void Awake()
        {
            UIManager.MainCanvas = GetComponent<Canvas>();
        }
    }
}
