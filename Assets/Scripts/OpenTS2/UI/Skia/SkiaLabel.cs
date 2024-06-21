using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkiaSharp;
using UnityEngine.UI;
using Codice.Client.Common;
using System.Runtime.InteropServices.WindowsRuntime;
using static OpenTS2.UI.Skia.ParsedLabelText;

namespace OpenTS2.UI.Skia
{
    [ExecuteAlways]
    [RequireComponent(typeof(RawImage))]
    public class SkiaLabel : MonoBehaviour
    {
        public ParsedLabelText ParsedText
        {
            get
            {
                if (_parsedText == null)
                    Render();
                return _parsedText;
            }
        }
        public int VerticalScroll
        {
            get
            {
                return m_VerticalScroll;
            }

            set
            {
                if (m_VerticalScroll != value)
                {
                    m_VerticalScroll = value;
                    Render();
                }
            }
        }
        public VerticalAlign VerticalAlign
        {
            get
            {
                return m_VerticalAlign;
            }

            set
            {
                if (m_VerticalAlign != value)
                {
                    m_VerticalAlign = value;
                    Render();
                }
            }
        }

        public HorizontalAlign HorizontalAlign
        {
            get
            {
                return m_HorizontalAlign;
            }

            set
            {
                if (m_HorizontalAlign != value)
                {
                    m_HorizontalAlign = value;
                    Render();
                }
            }
        }

        public SkiaFont Font
        {
            get
            {
                return m_Font;
            }

            set
            {
                m_FontAsset = null;
                _fontFromAsset = false;
                if (value != m_Font)
                {
                    m_Font = value;
                    Render();
                }
            }
        }

        public string Text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (value != m_Text)
                {
                    m_Text = value;
                    Render();
                }
            }
        }

        public int FontSize
        {
            get
            {
                return m_FontSize;
            }

            set
            {
                if (value != m_FontSize)
                {
                    m_FontSize = value;
                    Render();
                }
            }
        }

        public Color FontColor
        {
            get
            {
                return m_FontColor;
            }

            set
            {
                if (value != m_FontColor)
                {
                    m_FontColor = value;
                    Render();
                }
            }
        }

        public float LineSpacing
        {
            get
            {
                return m_LineSpacing;
            }

            set
            {
                if (value != m_LineSpacing)
                {
                    m_LineSpacing = value;
                    Render();
                }
            }
        }

        [TextArea]
        [SerializeField]
        protected string m_Text = "";
        [SerializeField]
        protected int m_FontSize = 32;
        [SerializeField]
        protected Color32 m_FontColor = Color.white;
        [SerializeField]
        protected float m_LineSpacing = 1f;
        [SerializeField]
        protected VerticalAlign m_VerticalAlign = VerticalAlign.Top;
        [SerializeField]
        protected HorizontalAlign m_HorizontalAlign = HorizontalAlign.Left;
        [SerializeField]
        protected int m_VerticalScroll = 0;
        protected SkiaFont m_Font;
        [SerializeField]
        private TextAsset m_FontAsset;
        [SerializeField]
        [HideInInspector]
        private bool _fontFromAsset = false;
        private SKPaint _skPaint = null;
        private ParsedLabelText _parsedText = null;

        protected RawImage RawImage
        {
            get
            {
                if (_rawImage == null)
                    _rawImage = GetComponent<RawImage>();
                return _rawImage;
            }
        }

        protected RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        protected Canvas Canvas
        {
            get
            {
                return GetComponentInParent<Canvas>();
            }
        }

        protected int PracticalWidth
        {
            get
            {
                float rectWidth = RectTransformUtility.PixelAdjustRect(RectTransform, Canvas).width;
                var ceil = Mathf.CeilToInt(rectWidth);
                ceil = Mathf.Max(ceil, 2);
                return ceil;
            }
        }

        protected int PracticalHeight
        {
            get
            {
                float rectHeight = RectTransformUtility.PixelAdjustRect(RectTransform, Canvas).height;
                var ceil = Mathf.CeilToInt(rectHeight);
                ceil = Mathf.Max(ceil, 2);
                return ceil;
            }
        }

        private Texture2D _texture;
        private RawImage _rawImage;
        private RectTransform _rectTransform;

        private void Awake()
        {
            var rawImage = GetComponent<RawImage>();
            if (rawImage == null)
                gameObject.AddComponent<RawImage>();
            Render();
        }

        private void OnValidate()
        {
            Render();
        }

        private void OnRectTransformDimensionsChange()
        {
            Render();
        }

        private void ValidateTexture(SKImageInfo imageInfo)
        {
            var fmt = (imageInfo.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
            var w = imageInfo.Width;
            var h = imageInfo.Height;

            w = Mathf.Max(w, 1);
            h = Mathf.Max(h, 1);

            if (_texture != null && !_texture.isReadable)
            {
                DestroyImmediate(_texture);
                _texture = null;
            }

            if (_texture == null)
            {
                _texture = new Texture2D(w, h, fmt, false, true);
            }
            else if (_texture.width != w || _texture.height != h || _texture.format != fmt)
            {
                _texture.Resize(w, h, fmt, false);
            }

            _texture.wrapMode = TextureWrapMode.Clamp;
            RawImage.texture = _texture;
            RawImage.uvRect = new Rect(0, 1, 1, -1);
        }

        private void ValidateFont()
        {
            if (m_FontAsset == null && m_Font != null && _fontFromAsset)
            {
                m_Font = null;
                _fontFromAsset = false;
            }
            if (m_FontAsset != null && m_Font == null)
            {
                m_Font = new SkiaFont(m_FontAsset.bytes);
                _fontFromAsset = true;
            }
        }

        public CharAtLine GetCharacterOnLine(int charIndex)
        {
            if (charIndex < 0 || charIndex >= _parsedText.OriginalText.Length)
                return null;

            for(var i = 0; i < _parsedText.Lines.Count; i++)
            {
                var line = _parsedText.Lines[i];
                if (charIndex >= line.EndIndex || charIndex < line.BeginIndex)
                    continue;
                var index = charIndex - line.BeginIndex;
                var charAtLine = new CharAtLine();
                charAtLine.CharIndex = index;
                charAtLine.LineIndex = i;
                return charAtLine;
            }

            var closestLeftLine = -1;

            for(var i = 0; i < _parsedText.Lines.Count; i++)
            {
                var line = _parsedText.Lines[i];
                if (line.EndIndex <= charIndex)
                    closestLeftLine = i;
                else
                    break;
            }

            if (closestLeftLine != -1)
            {
                var charAtLine = new CharAtLine();
                charAtLine.LineIndex = closestLeftLine + 1;
                charAtLine.CharIndex = 0;
                charAtLine.Underflows = true;
                return charAtLine;
            }

            return null;
        }

        private float AlignX(float x, int line)
        {
            var width = PracticalWidth;
            var lineWidth = _skPaint.MeasureText(ParsedText.Lines[line].Text);
            switch (HorizontalAlign)
            {
                case HorizontalAlign.Left:
                    break;

                case HorizontalAlign.Center:
                    x += width / 2f;
                    x -= lineWidth / 2f;
                    break;

                case HorizontalAlign.Right:
                    x += width;
                    x -= lineWidth;
                    break;
            }
            return x;
        }

        public CharRect GetCharacterRect(int charIndex)
        {
            if (charIndex < 0 || charIndex >= _parsedText.OriginalText.Length)
                return default;
            var charAtLine = GetCharacterOnLine(charIndex);
            if (charAtLine == null)
                charAtLine = GetCharacterOnLine(0);
            if (charAtLine.Underflows)
            {
                var x = 0f;
                var y = -GetLineY(charAtLine.LineIndex) + m_FontSize;
                var height = m_FontSize;
                var width = 1;

                return new CharRect(new Rect(x, y, width, height), IsLineOutOfBounds(charAtLine.LineIndex));
            }
            else
            {
                var line = _parsedText.Lines[charAtLine.LineIndex];
                var y = -GetLineY(charAtLine.LineIndex) + m_FontSize;
                var height = m_FontSize;

                var character = line.Text[charAtLine.CharIndex];
                var width = _skPaint.MeasureText(character.ToString());
                var subStringLeft = line.Text.Substring(0, charAtLine.CharIndex);
                var x = _skPaint.MeasureText(subStringLeft);
                x = AlignX(x, charAtLine.LineIndex);

                return new CharRect(new Rect(x, y, width, height), IsLineOutOfBounds(charAtLine.LineIndex));
            }
        }

        private void Render()
        {
            ValidateFont();
            var skImageInfo = new SKImageInfo(PracticalWidth, PracticalHeight);
            ValidateTexture(skImageInfo);

            var surface = SKSurface.Create(skImageInfo);
            var canvas = surface.Canvas;

            if (_skPaint == null)
                _skPaint = new SKPaint();

            _skPaint.TextSize = m_FontSize;
            _skPaint.IsAntialias = true;
            _skPaint.Color = new SKColor(m_FontColor.r, m_FontColor.g, m_FontColor.b, m_FontColor.a);
            _skPaint.IsStroke = false;

            if (m_Font != null && m_Font.Typeface != null)
                _skPaint.Typeface = m_Font.Typeface;
            else
                _skPaint.Typeface = SKTypeface.Default;
           
            DrawText(canvas, skImageInfo);

            var pixMap = surface.PeekPixels();
            _texture.LoadRawTextureData(pixMap.GetPixels(), pixMap.RowBytes * pixMap.Height);
            _texture.Apply(false, true);
        }
        public int GetTextCharIndexForLineChar(int lineIndex, int c)
        {
            var line = ParsedText.Lines[lineIndex];
            return c + line.BeginIndex;
        }

        private void ParseText(SKImageInfo imageInfo)
        {
            var parsedText = new ParsedLabelText();
            var text = m_Text;
            parsedText.OriginalText = text;
            text = text.Replace('\t', ' ');

            var wrappedHeight = (float)m_FontSize;

            var lines = new List<TextLine>();
            var lastWordIndex = -1;
            var lastLineIndex = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var currentText = text.Substring(lastLineIndex, (i + 1) - lastLineIndex);
                var cutText = currentText.Substring(0, currentText.Length - 1);
                var cutLine = new TextLine(cutText, lastLineIndex, lastLineIndex + (currentText.Length - 1));
                if (c == '\n' || c == '\r')
                {
                    lines.Add(cutLine);
                    lastLineIndex = i + 1;
                    lastWordIndex = -1;
                    continue;
                }

                if (c == ' ')
                {
                    lastWordIndex = i;
                }

                var length = _skPaint.MeasureText(currentText);
                if (length > imageInfo.Width)
                {
                    if (lastWordIndex != -1)
                    {
                        lastWordIndex++;
                        var textAtLastWord = text.Substring(lastLineIndex, lastWordIndex - lastLineIndex);
                        var lastWordLength = _skPaint.MeasureText(textAtLastWord);
                        if (lastWordLength <= imageInfo.Width)
                        {
                            var lineAtLastWord = new TextLine(textAtLastWord, lastLineIndex, lastWordIndex - 1);
                            lines.Add(lineAtLastWord);
                            var nextWordSearchStart = lastWordIndex;
                            lastLineIndex = lastWordIndex;
                            lastWordIndex = -1;
                            for (var n = nextWordSearchStart; n < text.Length; n++)
                            {
                                if (text[n] != ' ')
                                {
                                    lastLineIndex = n;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            lines.Add(cutLine);
                            lastLineIndex = i;
                            lastWordIndex = -1;
                        }
                    }
                    else
                    {
                        lines.Add(cutLine);
                        lastLineIndex = i;
                        lastWordIndex = -1;
                    }
                }
            }

            var lastLineLength = text.Length - lastLineIndex;
            if (lastLineLength > 0)
            {
                var lastLine = new TextLine(text.Substring(lastLineIndex), lastLineIndex, text.Length);
                lines.Add(lastLine);
            }

            wrappedHeight += (lines.Count - 1) * m_LineSpacing * m_FontSize;

            parsedText.Lines = lines;
            parsedText.Height = wrappedHeight;

            _parsedText = parsedText;
        }

        public float GetLineLength(int line)
        {
            return _skPaint.MeasureText(_parsedText.Lines[line].Text);
        }

        public float GetLineY(int line)
        {
            var height = PracticalHeight;
            var heightMiddle = height / 2f;
            var wrappedHeightMiddle = _parsedText.Height / 2f;
            var y = (float)m_FontSize;

            switch (VerticalAlign)
            {
                case VerticalAlign.Top:
                    y = m_FontSize;
                    break;

                case VerticalAlign.Middle:
                    y = (heightMiddle - wrappedHeightMiddle) + m_FontSize;
                    break;

                case VerticalAlign.Bottom:
                    y = (height - _parsedText.Height) + m_FontSize;
                    break;
            }

            y -= m_FontSize * LineSpacing * m_VerticalScroll;
            y += m_FontSize * m_LineSpacing * line;
            return y;
        }

        public bool IsLineOutOfBounds(int lineIndex)
        {
            var y = GetLineY(lineIndex);
            if (y < m_FontSize || y > PracticalHeight)
                return true;
            return false;
        }

        private void DrawText(SKCanvas canvas, SKImageInfo imageInfo)
        {
            ParseText(imageInfo);
            for(var i = 0; i < _parsedText.Lines.Count; i++)
            {
                var line = _parsedText.Lines[i];
                var width = imageInfo.Width;
                var middle = width / 2f;
                var lineLength = _skPaint.MeasureText(line.Text);
                var lineMiddle = lineLength / 2f;
                var xPos = 0f;

                switch (HorizontalAlign)
                {
                    case HorizontalAlign.Left:
                        xPos = 0f;
                        break;

                    case HorizontalAlign.Center:
                        xPos = middle - lineMiddle;
                        break;

                    case HorizontalAlign.Right:
                        xPos = width - lineLength;
                        break;
                }
                var y = GetLineY(i);
                if (!IsLineOutOfBounds(i))
                    canvas.DrawText(line.Text, xPos, y, _skPaint);
            }
        }

        private void OnDestroy()
        {
            if (_texture != null)
                DestroyImmediate(_texture);
        }
    }
}