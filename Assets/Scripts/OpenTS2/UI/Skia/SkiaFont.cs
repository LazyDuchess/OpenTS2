using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace OpenTS2.UI.Skia
{
    public class SkiaFont
    {
        internal SKTypeface Typeface = null;

        public SkiaFont(TextAsset asset)
        {
            var ms = new MemoryStream(asset.bytes);
            Typeface = SKTypeface.FromStream(ms);
        }

        public SkiaFont(byte[] data)
        {
            var ms = new MemoryStream(data);
            Typeface = SKTypeface.FromStream(ms);
        }

        public SkiaFont(string path)
        {
            Typeface = SKTypeface.FromFile(path);
        }
    }
}
