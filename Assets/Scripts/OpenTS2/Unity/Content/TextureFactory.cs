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
using UnityEngine;

namespace OpenTS2.Unity.Content
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
