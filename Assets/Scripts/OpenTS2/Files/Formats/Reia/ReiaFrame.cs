using OpenTS2.Engine;
using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Files.Formats.Reia
{
    public class ReiaFrame : IDisposable
    {
        private const int BlockSize = 32;
        private const int BlockSquare = BlockSize * BlockSize;

        public Texture2D Image => _image;
        private readonly Texture2D _image;

        public ReiaFrame(Texture2D image)
        {
            _image = image;
        }

        static IEnumerable<ReiaFrame> ReadFrameEnumerableInternal(IoBuffer io, int width, int height, bool reuseTexture)
        {
            Texture2D texture = null;
            Color32[] previousData = null;

            var frameMagic = io.ReadCString(4);
            while (io.HasMore && frameMagic != "")
            {
                if (frameMagic != "frme")
                {
                    throw new Exception("Magic in start of frame doesn't equal frme");
                }

                var frameSize = io.ReadUInt32();

                if (!reuseTexture || texture == null)
                    texture = new Texture2D(width, height);

                var frame = ReadSingleFrame(io, width, height, ref previousData, texture);

                yield return frame;

                var padding = frameSize % 2;
                io.Skip(padding);
                frameMagic = io.ReadCString(4);
            }
        }

        public static IEnumerable<ReiaFrame> ReadFrameEnumerable(IoBuffer io, int width, int height)
        {
            return ReadFrameEnumerableInternal(io, width, height, true);
        }

        public static List<ReiaFrame> ReadFrames(IoBuffer io, int width, int height)
        {
            var frameStream = ReadFrameEnumerableInternal(io, width, height, false);
            var frameList = new List<ReiaFrame>(frameStream);

            return frameList;
        }

        static Color32 ReadSinglePixel(IoBuffer io)
        {
            byte b = io.ReadByte();
            byte g = io.ReadByte();
            byte r = io.ReadByte();

            return new Color32(r, g, b, 255);
        }

        static void ReadBlock(IoBuffer io, Color32[] imgData)
        {
            var i = 0;
            while (i < BlockSquare)
            {
                var rleValue = io.ReadSByte();
                if (rleValue < 0)
                {
                    // Negative RLE value means we are going to be repeating the next
                    // color -n times.
                    var numRepeats = -rleValue;
                    var pixel = ReadSinglePixel(io);
                    for (var j = 0; j <= numRepeats; j++)
                    {
                        imgData[i++] = pixel;
                    }
                }
                else
                {
                    // Positive RLE value means we are going to be getting n unique
                    // pixels.
                    var numUniquePixels = rleValue;
                    for (var j = 0; j <= numUniquePixels; j++)
                    {
                        var pixel = ReadSinglePixel(io);
                        imgData[i++] = pixel;
                    }
                }
            }
        }

        static ReiaFrame ReadSingleFrame(IoBuffer io, int width, int height, ref Color32[] imageData, Texture2D image)
        {
            bool hadPrevious = imageData != null;

            if (!hadPrevious)
            {
                imageData = new Color32[width * height];

                for (int i = 0; i < imageData.Length; i++)
                {
                    imageData[i] = new Color32(0, 0, 0, 255);
                }
            }

            var maxJ = (int)Mathf.Ceil((float)width / BlockSize);
            var maxI = (int)Mathf.Ceil((float)height / BlockSize);
            image.wrapMode = TextureWrapMode.Clamp;

            Color32[] workArray = new Color32[BlockSquare];

            for (var i = 0; i < maxI; i++)
            {
                var y = (i * BlockSize);
                var yBound = y + BlockSize;
                var blockHeight = BlockSize;

                if (yBound > image.height)
                    blockHeight -= yBound - image.height;

                for (var j = 0; j < maxJ; j++)
                {
                    var x = (j * BlockSize);
                    var xBound = x + BlockSize;
                    var blockWidth = BlockSize;

                    if (xBound > image.width)
                        blockWidth -= xBound - image.width;

                    // First byte tells us if we should expect a new 32x32 pixel block or re-use
                    // the one from the previous frame.
                    var newFrame = io.ReadByte() != 0;

                    if (newFrame)
                    {
                        ReadBlock(io, workArray);

                        AddPixels(workArray, imageData, BlockSize, width, x, y, blockWidth, blockHeight);
                    }
                    else
                    {
                        if (!hadPrevious)
                        {
                            throw new Exception("Tried to re-use previous 32x32 block but there was none");
                        }

                        // Leave data unmodified.
                    }
                }
            }

            image.SetPixels32(imageData);
            image.Apply();
            return new ReiaFrame(image);
        }

        private static void AddPixels(Color32[] src, Color32[] dst, int srcStride, int dstStride, int dstX, int dstY, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                int srcOffset = y * srcStride;
                int dstOffset = dstX + (dstY + y) * dstStride;

                for (int x = 0; x < width; x++)
                {
                    Color32 srcPixel = src[srcOffset++];
                    ref Color32 dstPixel = ref dst[dstOffset++];

                    dstPixel.r += srcPixel.r;
                    dstPixel.g += srcPixel.g;
                    dstPixel.b += srcPixel.b;
                }
            }
        }

        public void Dispose()
        {
            if (_image != null)
            {
                _image.Free();
            }
        }
    }
}
