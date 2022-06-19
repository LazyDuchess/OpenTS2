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
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, string sourceFile)
        {
            // TODO - figure out JFIF ALFA block.
            // 0: TGA, 1: PNG, 2: JPG
            var fileType = 0;
            var pngCheck = Encoding.UTF8.GetString(bytes, 1, 3);
            var jpgCheck = Encoding.UTF8.GetString(bytes, 6, 4);
            if (pngCheck == "PNG")
                fileType = 1;
            else if (jpgCheck == "JFIF")
                fileType = 2;
            var textureAsset = new TextureAsset();
            object texture;
            switch (fileType)
            {
                case 0:
                texture = ContentManager.TextureFactory.CreateTGATexture(bytes);
                textureAsset.engineTexture = texture;
                return textureAsset;

                case 1:
                texture = ContentManager.TextureFactory.CreatePNGTexture(bytes);
                textureAsset.engineTexture = texture;
                return textureAsset;

                case 2:
                texture = ContentManager.TextureFactory.CreateJPGTexture(bytes);
                textureAsset.engineTexture = texture;
                return textureAsset;
            }
            return textureAsset;
        }
    }
}