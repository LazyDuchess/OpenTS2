using OpenTS2.UI.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.UI
{
    public class FontStyle
    {
        public int Size = 14;
        public SkiaFont Font;

        public FontStyle(SkiaFont font, int size)
        {
            Font = font;
            Size = size;
        }

        public void Apply(SkiaLabel label)
        {
            label.Font = Font;
            label.FontSize = Size;
        }
    }
}
