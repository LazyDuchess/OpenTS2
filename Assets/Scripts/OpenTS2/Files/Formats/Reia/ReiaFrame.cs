using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Files.Formats.Reia
{
    public class ReiaFrame
    {
        public Texture2D Image => _image;
        private readonly Texture2D _image;

        public ReiaFrame(Texture2D Image)
        {
            _image = Image;
        }
        public static List<ReiaFrame> ReadFrames(IoBuffer io, int width, int height)
        {
            var frameList = new List<ReiaFrame>();
            ReiaFrame previousFrame = null;

            var frameMagic = io.ReadCString(4);
            while (io.HasMore && frameMagic != "")
            {
                if (frameMagic != "frme")
                {
                    throw new Exception("Magic in start of frame doesn't equal frme");
                }

                var frameSize = io.ReadUInt32();

                var frame = ReadSingleFrame(io, width, height, previousFrame?.Image);

                previousFrame = frame;

                frameList.Add(frame);

                var padding = frameSize % 2;
                io.Skip(padding);
                frameMagic = io.ReadCString(4);
            }
            return frameList;
        }

        static Color32 ReadSinglePixel(IoBuffer io)
        {
            var b = io.ReadByte();
            var g = io.ReadByte();
            var r = io.ReadByte();
            return new Color32(r, g, b, byte.MaxValue);
        }

        static Texture2D Read32By32Block(IoBuffer io)
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
            var tex = new Texture2D(32, 32);
            tex.SetPixels32(imgData);
            tex.Apply();
            return tex;
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

        static ReiaFrame ReadSingleFrame(IoBuffer io, int width, int height, Texture2D previousFrame)
        {
            var image = new Texture2D(width, height);
            var maxI = (int)Mathf.Floor(width / 32);
            var maxJ = (int)Mathf.Floor(height / 32);
            for (var i = 0; i < maxI; i++)
            {
                for (var j = 0; j < maxJ; j++)
                {
                    var x = (j * 32);
                    var y = (i * 32);

                    // First byte tells us if we should expect a new 32x32 pixel block or re-use
                    // the one from the previous frame.
                    var newFrame = io.ReadByte() != 0;

                    if (newFrame)
                    {
                        var block = Read32By32Block(io);

                        if (previousFrame != null)
                        {
                            var previousPixels = previousFrame.GetPixels(x, y, 32, 32);
                            var previousBlock = new Texture2D(32, 32);
                            previousBlock.SetPixels(previousPixels);
                            previousBlock.Apply();
                            AddTextures(previousBlock, block);
                        }
                        image.SetPixels(x, y, 32, 32, block.GetPixels());
                    }
                    else
                    {
                        if (previousFrame == null)
                        {
                            throw new Exception("Tried to re-use previous 32x32 block but it's null");
                        }
                        var previousPixels = previousFrame.GetPixels(x, y, 32, 32);
                        var previousBlock = new Texture2D(32, 32);
                        previousBlock.SetPixels(previousPixels);
                        previousBlock.Apply();
                        image.SetPixels(x, y, 32, 32, previousBlock.GetPixels());
                    }
                }
            }
            image.Apply();
            return new ReiaFrame(image);
        }
    }
}
