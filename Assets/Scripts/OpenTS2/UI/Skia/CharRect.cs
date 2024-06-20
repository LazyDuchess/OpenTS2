using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Skia
{
    public class CharRect
    {
        public Rect Rect;
        public bool OutOfBounds = false;

        public CharRect(Rect rect, bool outOfBounds)
        {
            Rect = rect;
            OutOfBounds = outOfBounds;
        }
    }
}
