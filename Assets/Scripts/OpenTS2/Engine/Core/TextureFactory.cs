/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content;
using OpenTS2.Content.Interfaces;
using OpenTS2.Files.Formats.ARC;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Engine.Core
{
    /// <summary>
    /// Constructs Unity Texture2D resources.
    /// </summary>
    public class TextureFactory : ITextureFactory
    {
        public override object CreateJPGTexture(byte[] source)
        {
            Texture2D fTex = new Texture2D(1, 1);
            fTex.LoadImage(source);
            // TODO - Figure out how the fuck jpg alpha works. Preferably on the IMG codec, not here.
            /*
            var alphaCheck = Encoding.UTF8.GetString(source, 24, 4);
            if (alphaCheck == "ALFA")
            {
                var alphaTex = new Texture2D(fTex.width, fTex.height);
                var io = IoBuffer.FromBytes(source);
                io.Seek(SeekOrigin.Begin, 24 + 4);
                var transparentPixels = new List<Tuple<byte, byte>>();
                var donePixels = 0;
                
                var pixels = new List<Color>();
                while (donePixels <= fTex.width*fTex.height)
                {
                    var len = io.ReadByte();
                    var value = (float)io.ReadByte() / (float)byte.MaxValue;
                    for (var i=0;i<len;i++)
                        pixels.Add(new Color(value,value,value));
                    donePixels += len;
                }
                var currentY = 0;
                var currentX = 0;
                for (var i=0;i<pixels.Count;i++)
                {
                    alphaTex.SetPixel(currentX, currentY, pixels[i]);
                    currentX += 1;
                    if (currentX > alphaTex.width)
                    {
                        currentX = 0;
                        currentY += 1;
                    }    
                }
                alphaTex.Apply();
                return alphaTex;
            }*/
            return fTex;
        }

        public override object CreatePNGTexture(byte[] source)
        {
            Texture2D fTex = new Texture2D(1, 1);
            fTex.LoadImage(source);
            return fTex;
        }

        public override object CreateTexture(PalettizedARCTexture source)
        {
            var texture = new Texture2D(source.width, source.height);
            for (var i = 0; i < source.width; i++)
            {
                for (var n = 0; n < source.height; n++)
                {
                    var col = source.palette[source.pixels[n + i * source.width]];
                    texture.SetPixel(n, i, col.UnityColor);
                }
            }
            return texture;
        }

        public override object CreateTGATexture(byte[] source)
        {
            return new Paloma.TargaImage(source).bmpTargaImage;
        }
    }
}
