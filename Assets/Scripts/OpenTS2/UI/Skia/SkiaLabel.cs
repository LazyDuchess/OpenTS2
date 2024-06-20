using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkiaSharp;
using UnityEngine.UI;
using Codice.Client.Common;
using System.Runtime.InteropServices.WindowsRuntime;

namespace OpenTS2.UI.Skia
{
    [ExecuteAlways]
    [RequireComponent(typeof(RawImage))]
    public class SkiaLabel : MonoBehaviour
    {
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
                return FontSize;
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
        protected SkiaFont m_Font;
        [SerializeField]
        private TextAsset m_FontAsset;
        [SerializeField]
        [HideInInspector]
        private bool _fontFromAsset = false;

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

        private void Render()
        {
            ValidateFont();
            var skImageInfo = new SKImageInfo(PracticalWidth, PracticalHeight);
            ValidateTexture(skImageInfo);

            var surface = SKSurface.Create(skImageInfo);
            var canvas = surface.Canvas;

            using (var paint = new SKPaint())
            {
                paint.TextSize = m_FontSize;
                paint.IsAntialias = true;
                paint.Color = new SKColor(m_FontColor.r, m_FontColor.g, m_FontColor.b, m_FontColor.a);
                paint.IsStroke = false;

                if (m_Font != null && m_Font.Typeface != null)
                    paint.Typeface = m_Font.Typeface;

                DrawText(canvas, paint, skImageInfo);
            }

            var pixMap = surface.PeekPixels();
            _texture.LoadRawTextureData(pixMap.GetPixels(), pixMap.RowBytes * pixMap.Height);
            _texture.Apply(false, true);
        }

        private void DrawText(SKCanvas canvas, SKPaint paint, SKImageInfo imageInfo)
        {
            var text = m_Text;
            text = text.Replace("\t", "    ");

            var wrappedHeight = (float)m_FontSize;

            var lines = new List<string>();
            var lastCharWasSpace = false;
            var lastWordIndex = -1;
            var lastLineIndex = 0;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var currentText = text.Substring(lastLineIndex, (i + 1) - lastLineIndex);
                var cutText = currentText.Substring(0, currentText.Length - 1);

                if (c == '\n' || c == '\r')
                {
                    lines.Add(cutText);
                    lastLineIndex = i + 1;
                    lastWordIndex = -1;
                    lastCharWasSpace = false;
                    continue;
                }

                if (c == ' ')
                {
                    if (!lastCharWasSpace)
                        lastWordIndex = i;
                    lastCharWasSpace = true;
                    continue;
                }

                lastCharWasSpace = false;
                var length = paint.MeasureText(currentText);
                if (length > imageInfo.Width)
                {
                    if (lastWordIndex != -1)
                    {
                        var textAtLastWord = text.Substring(lastLineIndex, lastWordIndex - lastLineIndex);
                        var lastWordLength = paint.MeasureText(textAtLastWord);
                        if (lastWordLength <= imageInfo.Width)
                        {
                            lines.Add(textAtLastWord);
                            var nextWordSearchStart = lastWordIndex + 1;
                            lastLineIndex = lastWordIndex + 1;
                            lastWordIndex = -1;
                            lastCharWasSpace = false;
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
                            lines.Add(cutText);
                            lastLineIndex = i;
                            lastWordIndex = -1;
                            lastCharWasSpace = false;
                        }
                    }
                    else
                    {
                        lines.Add(cutText);
                        lastLineIndex = i;
                        lastWordIndex = -1;
                        lastCharWasSpace = false;
                    }
                }
            }

            var lastLineLength = text.Length - lastLineIndex;
            if (lastLineLength > 0)
            {
                lines.Add(text.Substring(lastLineIndex));
            }

            wrappedHeight += (lines.Count - 1) * m_LineSpacing * m_FontSize;

            var height = imageInfo.Height;
            var heightMiddle = height / 2f;
            var wrappedHeightMiddle = wrappedHeight / 2f;

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
                    y = (height - wrappedHeight) + m_FontSize;
                    break;
            }

            foreach (var wrappedLine in lines)
            {
                var width = imageInfo.Width;
                var middle = width/2f;
                var lineLength = paint.MeasureText(wrappedLine);
                var lineMiddle = lineLength/2f;
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
                if (y >= m_FontSize && y <= height)
                    canvas.DrawText(wrappedLine, xPos, y, paint);
                y += m_FontSize * m_LineSpacing;
            }
        }

        private void OnDestroy()
        {
            if (_texture != null)
                DestroyImmediate(_texture);
        }
    }
}