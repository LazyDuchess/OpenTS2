/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using OpenTS2.Files.Utils;
using OpenTS2.Content;
using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using System.Text;

namespace OpenTS2.Files.Formats.DBPF
{

    /// <summary>
    /// IMG file reading codec.
    /// </summary>
    public class IMGCodec : AbstractCodec
    {

        /// <summary>
        /// Constructs a new IMG instance.
        /// </summary>
        public IMGCodec()
        {

        }

        /// <summary>
        /// Parses IMG from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override AbstractAsset Deserialize(byte[] bytes, TGI tgi, string sourceFile)
        {
            var pngCheck = Encoding.UTF8.GetString(bytes, 1, 3);
            var jpgCheck = Encoding.UTF8.GetString(bytes, 6, 4);
            var textureAsset = new TextureAsset();
            object texture;
            if (pngCheck != "PNG" && jpgCheck != "JFIF")
            {
                texture = ContentManager.TextureFactory.CreateTGATexture(bytes);
                textureAsset.engineTexture = texture;
                return textureAsset;
            }
            if (pngCheck == "PNG")
            {
                texture = ContentManager.TextureFactory.CreatePNGTexture(bytes);
                textureAsset.engineTexture = texture;
                return textureAsset;
            }
            texture = ContentManager.TextureFactory.CreateJPGTexture(bytes);
            textureAsset.engineTexture = texture;
            return textureAsset;
        }
    }
}