using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.UI.Skia
{
    public class TextLine
    {
        public string Text;
        public int BeginIndex;
        public int EndIndex;

        public TextLine(string text, int beginIndex, int endIndex)
        {
            Text = text;
            BeginIndex = beginIndex;
            EndIndex = endIndex;
        }
    }
}
