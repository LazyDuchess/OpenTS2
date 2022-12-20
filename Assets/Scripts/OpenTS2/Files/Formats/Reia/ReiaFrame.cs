using OpenTS2.Engine;
using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Files.Formats.Reia
{
    public class ReiaFrame : IDisposable
    {
        public Texture2D Image => _image;
        private readonly Texture2D _image;

        public ReiaFrame(Texture2D Image)
        {
            _image = Image;
        }

        static IEnumerable<ReiaFrame> ReadFrameEnumerableInternal(IoBuffer io, int width, int height, bool disposePreviousFrames)
        {
            ReiaFrame previousFrame = null;

            var frameMagic = io.ReadCString(4);
            while (io.HasMore && frameMagic != "")
            {
                if (frameMagic != "frme")
                {
                    throw new Exception("Magic in start of frame doesn't equal frme");
                }

                var frameSize = io.ReadUInt32();

                var currentFrame = previousFrame?.Image;

                if (!disposePreviousFrames || currentFrame == null)
                    currentFrame = new Texture2D(width, height);


                var frame = ReadSingleFrame(io, width, height, previousFrame?.Image, currentFrame);
                /*
                if (disposePreviousFrames)
                    previousFrame?.Dispose();*/

                previousFrame = frame;

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
            var bytes = io.ReadBytes(3);
            return new Color32(bytes[2], bytes[1], bytes[0], 255);
        }

        static Texture2D ReadBlock(IoBuffer io, int blockWidth, int blockHeight)
        {
            var numPixels = 32 * 32;

            // 3 bytes for the RGB channels per pixel.
            var imgData = new Color32[numPixels];

            var i = 0;
            while (i < numPixels)
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
                        imgData[i] = pixel;
                        i++;
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
                        imgData[i] = pixel;
                        i++;
                    }
                }
            }
            if (blockWidth != 32 || blockHeight != 32)
            {
                var texTemp = new Texture2D(32, 32);
                texTemp.SetPixels32(imgData);
                texTemp.Apply();
                var trimmedImgData = texTemp.GetPixels(0, 0, blockWidth, blockHeight);
                texTemp.Free();
                var tex = new Texture2D(blockWidth, blockHeight);
                tex.SetPixels(trimmedImgData);
                tex.Apply();
                return tex;
            }
            else
            {
                var tex = new Texture2D(blockWidth, blockHeight);
                tex.SetPixels32(0, 0, blockWidth, blockHeight, imgData);
                tex.Apply();
                return tex;
            }
        }

        static void AddTextures(Texture2D source, Texture2D destination)
        {
            var destinationPixels = destination.GetPixels32();
            var sourcePixels = source.GetPixels32();
            for (var i = 0; i < destinationPixels.Length; i++)
            {
                var newPixel = destinationPixels[i];
                newPixel.r += sourcePixels[i].r;
                newPixel.g += sourcePixels[i].g;
                newPixel.b += sourcePixels[i].b;
                destinationPixels[i] = newPixel;
            }
            destination.SetPixels32(destinationPixels);
            destination.Apply();
        }

        static ReiaFrame ReadSingleFrame(IoBuffer io, int width, int height, Texture2D previousFrame, Texture2D image)
        {
            var maxJ = (int)Mathf.Ceil((float)width / 32);
            var maxI = (int)Mathf.Ceil((float)height / 32);
            image.wrapMode = TextureWrapMode.Clamp;


            for (var i = 0; i < maxI; i++)
            {
                for (var j = 0; j < maxJ; j++)
                {
                    var x = (j * 32);
                    var y = (i * 32);

                    var xBound = x + 32;
                    var yBound = y + 32;

                    var blockWidth = 32;
                    var blockHeight = 32;

                    if (xBound > image.width)
                        blockWidth -= xBound - image.width;
                    if (yBound > image.height)
                        blockHeight -= yBound - image.height;

                    Texture2D smallBlock = new Texture2D(blockWidth, blockHeight);

                    // First byte tells us if we should expect a new 32x32 pixel block or re-use
                    // the one from the previous frame.
                    var newFrame = io.ReadByte() != 0;

                    if (newFrame)
                    {
                        var block = ReadBlock(io, blockWidth, blockHeight);

                        if (previousFrame != null)
                        {
                            var previousPixels = previousFrame.GetPixels(x, y, blockWidth, blockHeight);
                            smallBlock.SetPixels(previousPixels);
                            smallBlock.Apply();
                            AddTextures(smallBlock, block);
                        }
                        image.SetPixels32(x, y, blockWidth, blockHeight, block.GetPixels32());
                        block.Free();
                    }
                    else
                    {
                        if (previousFrame == null)
                        {
                            throw new Exception("Tried to re-use previous 32x32 block but it's null");
                        }
                        var previousPixels = previousFrame.GetPixels(x, y, blockWidth, blockHeight);
                        smallBlock.SetPixels(previousPixels);
                        smallBlock.Apply();
                        image.SetPixels32(x, y, blockWidth, blockHeight, smallBlock.GetPixels32());
                    }
                    if (smallBlock != null)
                        smallBlock.Free();
                }
            }

            image.Apply();
            return new ReiaFrame(image);
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
