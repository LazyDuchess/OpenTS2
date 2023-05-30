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
using OpenTS2.Files.Formats.JPEGWithAlfaSegment;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{

    /// <summary>
    /// IMG file reading codec.
    /// </summary>
    [Codec(TypeIDs.IMG, TypeIDs.IMG2)]
    public class IMGCodec : AbstractCodec
    {

        /// <summary>
        /// Constructs a new IMG instance.
        /// </summary>
        public IMGCodec()
        {

        }

        Texture2D CreateJPGTexture(byte[] source)
        {
            Texture2D tex2D = new Texture2D(1, 1);
            tex2D.LoadImage(source);
            return tex2D;
        }

        Texture2D CreateJPGTextureWithAlphaChannel(
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

        Texture2D CreatePNGTexture(byte[] source)
        {
            Texture2D fTex = new Texture2D(1, 1);
            fTex.LoadImage(source);
            return fTex;
        }

        Texture2D CreateTGATexture(byte[] source)
        {
            var texture = new Paloma.TargaImage(source, false).bmpTargaImage;
            return texture;
        }

        /// <summary>
        /// Parses IMG from an array of bytes.
        /// </summary>
        /// <param name="bytes">Bytes to parse</param>
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            // 0: TGA, 1: PNG, 2: JPG
            var fileType = 0;
            var formatName = "";
            var pngCheck = Encoding.UTF8.GetString(bytes, 1, 3);
            var jpgCheck = Encoding.UTF8.GetString(bytes, 6, 4);
            if (pngCheck == "PNG")
                fileType = 1;
            else if (jpgCheck == "JFIF")
                fileType = 2;
            TextureAsset asset = null;
            switch (fileType)
            {
                case 0:
                    asset = new TextureAsset(CreateTGATexture(bytes));
                    formatName = "TGA";
                    break;
                case 1:
                    asset = new TextureAsset(CreatePNGTexture(bytes));
                    formatName = "PNG";
                    break;
                case 2:
                    {
                        byte[] alphaChannel = JpegFileWithAlfaSegment.GetTransparencyFromAlfaSegment(bytes);
                        if (alphaChannel != null)
                        {
                            return new TextureAsset(
                                CreateJPGTextureWithAlphaChannel(bytes, alphaChannel));
                        }

                        asset = new TextureAsset(CreateJPGTexture(bytes));
                        formatName = "JPG";
                    }
                    break;
            }
            if (asset?.Texture != null)
                asset.Texture.name = $"[{formatName}] {tgi}";
            return asset;
        }
    }
}