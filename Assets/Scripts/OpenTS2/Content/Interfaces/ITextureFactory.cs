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
using OpenTS2.Files.Formats.ARC;

namespace OpenTS2.Content.Interfaces
{
    /// <summary>
    /// Generates engine textures from deserialized game textures.
    /// </summary>
    public abstract class ITextureFactory
    {
        /// <summary>
        /// Creates an engine texture out of a palettized ARC Texture.
        /// </summary>
        /// <param name="source">ARC Texture source.</param>
        /// <returns></returns>
        public abstract object CreateTexture(PalettizedARCTexture source);

        public abstract object CreateTGATexture(byte[] source);

        public abstract object CreatePNGTexture(byte[] source);

        public abstract object CreateJPGTexture(byte[] source);
    }
}
