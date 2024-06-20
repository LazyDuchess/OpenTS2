using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkiaSharp;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class SkiaLabel : MonoBehaviour
{
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

        if (_texture != null && !_texture.isReadable)
        {
            DestroyImmediate(_texture);
            _texture = null;
        }

        if (_texture == null)
        {
            _texture = new Texture2D(imageInfo.Width, imageInfo.Height, fmt, false, true);
        }
        else if (_texture.width != imageInfo.Width || _texture.height != imageInfo.Height || _texture.format != fmt)
        {
            _texture.Resize(imageInfo.Width, imageInfo.Height, fmt, false);
        }

        _texture.wrapMode = TextureWrapMode.Clamp;
        RawImage.texture = _texture;
        RawImage.uvRect = new Rect(0, 1, 1, -1);
    }

    private void Render()
    {
        var skImageInfo = new SKImageInfo(Mathf.CeilToInt(RectTransform.sizeDelta.x), Mathf.CeilToInt(RectTransform.sizeDelta.y));
        ValidateTexture(skImageInfo);

        var surface = SKSurface.Create(skImageInfo);
        var canvas = surface.Canvas;

        using (var paint = new SKPaint())
        {
            paint.TextSize = m_FontSize;
            paint.IsAntialias = true;
            paint.Color = new SKColor(m_FontColor.r, m_FontColor.g, m_FontColor.b, m_FontColor.a);
            paint.IsStroke = false;

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

        var lines = new List<string>();
        var lastCharWasSpace = false;
        var lastWordIndex = -1;
        var lastLineIndex = 0;
        for(var i = 0; i < text.Length; i++)
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

        var y = (float)m_FontSize;
        foreach (var wrappedLine in lines)
        {
            canvas.DrawText(wrappedLine, 0f, y, paint);
            y += m_FontSize * m_LineSpacing;
        }
    }

    private void OnDestroy()
    {
        if (_texture != null)
            DestroyImmediate(_texture);
    }
}
