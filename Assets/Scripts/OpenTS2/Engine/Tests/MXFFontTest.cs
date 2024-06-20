using OpenTS2.Files;
using OpenTS2.Files.Formats.Font;
using OpenTS2.UI.Skia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class MXFFontTest : MonoBehaviour
    {
        public string BaseGameFontToUse = "";
        public SkiaLabel Label;

        private void Awake()
        {
            var baseGamePath = Filesystem.GetDataPathForProduct(Content.ProductFlags.BaseGame);
            var fontPath = Path.Combine(baseGamePath, $"Res/UI/Fonts/{BaseGameFontToUse}");
            var mxf = new MXFFile(fontPath);
            var font = new SkiaFont(mxf.DecodedData);
            Label.Font = font;
        }
    }
}
