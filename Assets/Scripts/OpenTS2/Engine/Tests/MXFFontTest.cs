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
        public RectTransform Selector;
        public SkiaLabel SelectableLabel;
        public int SelectedCharacter = 0;
        private int _previousCharacter = 0;

        private void Start()
        {
            var baseGamePath = Filesystem.GetDataPathForProduct(Content.ProductFlags.BaseGame);
            var fontPath = Path.Combine(baseGamePath, $"Res/UI/Fonts/{BaseGameFontToUse}");
            var mxf = new MXFFile(fontPath);
            var font = new SkiaFont(mxf.DecodedData);
            Label.Font = font;
        }

        private void Update()
        {
            var rect = SelectableLabel.GetCharacterRect(SelectedCharacter);
            Selector.anchoredPosition = new Vector2(rect.Rect.x, rect.Rect.y);
            Selector.sizeDelta = new Vector2(rect.Rect.width, rect.Rect.height);
            Selector.gameObject.SetActive(!rect.OutOfBounds);
            if (SelectedCharacter != _previousCharacter)
            {
                _previousCharacter = SelectedCharacter;
                Debug.Log($"Selecting {SelectableLabel.ParsedText.OriginalText[SelectedCharacter]}");
            }
        }
    }
}
