/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

#define REMOVE_STRAY_PIXELS

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

namespace OpenTS2.Engine
{
    /// <summary>
    /// Constructs Unity Texture2D resources.
    /// </summary>
    public class TextureFactory : ITextureFactory
    {

        public override object CreateJPGTexture(byte[] source)
        {
            Texture2D tex2D = new Texture2D(1, 1);
            tex2D.LoadImage(source);
            return tex2D;
        }

        public override object CreateJPGTextureWithAlphaChannel(
            byte[] source, byte[] alphaChannel)
        {
            Texture2D tex2D = new Texture2D(1, 1);
            tex2D.LoadImage(source);
            Debug.Assert(tex2D.width * tex2D.height == alphaChannel.Length);

            var pixels = tex2D.GetPixels();

            // Unity flattens the pixels left-to-right bottom-to-top but the alpha channel
            // is top-to-bottom left-to-right.
            var alphaCounter = 0;
            for (var i = tex2D.height - 1; i >= 0; i--)
            {
                for (var j = 0; j < tex2D.width; j++)
                {
                    byte transparency = alphaChannel[alphaCounter];
                    alphaCounter++;

                    // Unity Color uses floats from 0 to 1, the alpha
                    // channel is bytes from 0 to 0xff.
                    pixels[i * tex2D.height + j].a = (float)transparency / 0xff;
                }
            }

            var alphaTex = new Texture2D(tex2D.width, tex2D.height, TextureFormat.RGBA32, true);
            alphaTex.SetPixels(pixels);
            alphaTex.Apply();

            return alphaTex;
        }

        public override object CreatePNGTexture(byte[] source)
        {
            Texture2D fTex = new Texture2D(1, 1);
            fTex.LoadImage(source);
            return fTex;
        }

        public override object CreateTexture(PalettizedARCTexture source)
        {
            var texture = new Texture2D(source.Width, source.Height);
            for (var i = 0; i < source.Width; i++)
            {
                for (var n = 0; n < source.Height; n++)
                {
                    var col = source.Palette[source.Pixels[n + i * source.Width]];
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
