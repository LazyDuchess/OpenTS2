/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content;
using UnityEngine;
using OpenTS2.Files.Formats.ARC;

namespace OpenTS2.Unity.Content
{
    /// <summary>
    /// Represents a Unity Texture2D
    /// </summary>
    public class UnityTexture : AbstractEngineTexture
    {
        public Texture2D texture;
        public UnityTexture(PalettizedARCTexture source)
        {
            if (texture == null)
            {
                texture = new Texture2D(source.width, source.height);
                for (var i = 0; i < source.width; i++)
                {
                    for (var n = 0; n < source.height; n++)
                    {
                        var col = source.palette[source.pixels[n + i * source.width]];
                        texture.SetPixel(n, i, col.UnityColor);
                    }
                }
            }
        }

    }
}
