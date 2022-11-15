using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class ALFATest : MonoBehaviour
    {
        public string SourceFile = "E:/TransparencyTest.png";
        public string OutputFile = "E:/TransparencyOut.alfa";
        private void Start()
        {
            var tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(SourceFile));
            var outStream = new FileStream(OutputFile, FileMode.Create);
            var outWriter = new BinaryWriter(outStream);
            outWriter.Write((ushort)0);
            outWriter.Write('A');
            outWriter.Write('L');
            outWriter.Write('F');
            outWriter.Write('A');
            var startPos = outStream.Position;
            var currentX = 0;
            var currentY = 0;
            var len = 0;
            var w = tex.width;
            var h = tex.height;
            var lastColorSet = false;
            var lastColor = Color.white;
            while (currentY < h)
            {
                if (currentX >= w)
                {
                    currentY += 1;
                    currentX -= w;
                }
                if (!lastColorSet)
                {
                    lastColorSet = true;
                    lastColor = tex.GetPixel(currentX, currentY);
                    len = 1;
                    currentX += 1;
                }
                else
                {
                    var col = tex.GetPixel(currentX, currentY);
                    if (col != lastColor)
                    {
                        if (lastColorSet && len > 0)
                        {
                            outWriter.Write((byte)len);
                            outWriter.Write((byte)Math.Round(lastColor.r * 255));
                        }
                        lastColor = col;
                        //len = 1;
                        currentX += 1;
                    }
                    else
                    {
                        currentX += 1;
                        len += 1;
                        if (currentX >= w)
                        {
                            currentY += 1;
                            currentX -= w;
                        }
                        if (len >= 128)
                        {
                            if (lastColorSet && len > 0)
                            {
                                outWriter.Write((byte)len);
                                outWriter.Write((byte)Math.Round(lastColor.r * 255));
                            }
                            lastColorSet = false;
                            len = 0;
                        }

                    }
                }
            }
            outWriter.Write((byte)0x08);
            var finalPos = outStream.Position;
            var finalSize = (ushort)(finalPos - startPos);
            outStream.Position = 0;
            outWriter.Write(finalSize);
            outWriter.Dispose();
        }
    }
}
