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
using OpenTS2.Content.Interfaces;
using OpenTS2.Files.Formats.ARC;

namespace OpenTS2.Unity.Content
{
    public class TextureFactory : AbstractTextureFactory
    {
        public override AbstractEngineTexture CreateTexture(PalettizedARCTexture source)
        {
            return new UnityTexture(source);
        }
    }
}
