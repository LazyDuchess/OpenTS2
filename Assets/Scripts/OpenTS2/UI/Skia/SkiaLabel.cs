using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkiaSharp;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class SkiaLabel : MonoBehaviour
{
    [TextArea]
    public string Text = "";
    public int FontSize = 32;
    public Color32 FontColor = Color.white;
    private Texture2D _texture;

    private void Awake()
    {
        Render();
    }

    private void ValidateTexture(SKImageInfo imageInfo)
    {
        var rawImage = GetComponent<RawImage>();
        var fmt = (imageInfo.ColorType == SKColorType.Rgba8888) ? TextureFormat.RGBA32 : TextureFormat.BGRA32;
        
        if (_texture == null)
        {
            _texture = new Texture2D(imageInfo.Width, imageInfo.Height, fmt, false, true);
        }
        else if (_texture.width != imageInfo.Width || _texture.height != imageInfo.Height || _texture.format != fmt)
        {
            _texture.Resize(imageInfo.Width, imageInfo.Height, fmt, false);
        }

        _texture.wrapMode = TextureWrapMode.Clamp;
        rawImage.texture = _texture;
    }

    private void Render()
    {
        var rect = GetComponent<RectTransform>();
        var skImageInfo = new SKImageInfo(Mathf.CeilToInt(rect.sizeDelta.x), Mathf.CeilToInt(rect.sizeDelta.y));
        ValidateTexture(skImageInfo);

        var surface = SKSurface.Create(skImageInfo);
        var canvas = surface.Canvas;

        using (var paint = new SKPaint())
        {
            paint.TextSize = FontSize;
            paint.IsAntialias = true;
            paint.Color = new SKColor(FontColor.r, FontColor.g, FontColor.b, FontColor.a);
            paint.IsStroke = false;

            DrawText(canvas, paint, skImageInfo);
        }

        var pixMap = surface.PeekPixels();
        _texture.LoadRawTextureData(pixMap.GetPixels(), pixMap.RowBytes * pixMap.Height);
        _texture.Apply(false, true);
    }

    private void DrawText(SKCanvas canvas, SKPaint paint, SKImageInfo imageInfo)
    {
        var splitText = Text.Split(' ');
        var wrappedLines = new List<string>();
        var lineLength = 0f;
        var line = "";
        foreach(var word in splitText)
        {
            var wordWithSpace = $"{word} ";
            var wordWithSpaceLength = paint.MeasureText(wordWithSpace);
            if (lineLength + wordWithSpaceLength > imageInfo.Width)
            {
                wrappedLines.Add(line);
                line = wordWithSpace;
                lineLength = wordWithSpaceLength;
            }
            else
            {
                line += wordWithSpace;
                lineLength += wordWithSpaceLength;
            }
        }

        wrappedLines.Add(line);

        var y = FontSize;
        foreach (var wrappedLine in wrappedLines)
        {
            Debug.Log(wrappedLine);
            canvas.DrawText(wrappedLine, 0f, y, paint);
            y += FontSize;
        }
    }

    private void OnDestroy()
    {
        if (_texture != null)
            DestroyImmediate(_texture);
    }
}
