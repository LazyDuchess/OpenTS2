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
using OpenTS2.Common;
using System.IO;
using OpenTS2.Files.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;

namespace OpenTS2.Files.Formats.ARC
{
    /// <summary>
    /// An 8 bpp texture with a 256 color palette.
    /// </summary>
    public class PalettizedARCTexture
    {
        public int width = 0;
        public int height = 0;
        public List<Color> palette = new List<Color>();
        public List<int> pixels = new List<int>();
    }

    /// <summary>
    /// Codec for Playstation 2 textures.
    /// </summary>
    public class ARCTextureCodec
    {
        
        /// <summary>
        /// Deserializes an ARC Texture byte array into a Texture asset.
        /// </summary>
        /// <param name="bytes">ARC Texture bytes.</param>
        /// <returns>A Texture asset.</returns>
        public TextureAsset Deserialize(byte[] bytes)
        {
            var textureAsset = new TextureAsset();
            var texture = new PalettizedARCTexture();
            var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN);
            //pad
            io.Skip(8);
            //magic
            io.Skip(4);
            //pad
            io.Skip(4);
            //sizes
            texture.width = io.ReadUInt16();
            texture.height = io.ReadUInt16();
            //dunno
            io.Skip(4);
            //bit flags?
            io.Skip(4);
            //pad
            io.Skip(4);
            var colorPos = io.Position;
            io.Skip(texture.width * texture.height);
            for (var i = 0; i < 256; i++)
            {
                var R = (float)io.ReadByte() / (float)byte.MaxValue;
                var G = (float)io.ReadByte() / (float)byte.MaxValue;
                var B = (float)io.ReadByte() / (float)byte.MaxValue;
                var A = (float)io.ReadByte() / 128f;
                var col = new Color(R, G, B, A);
                texture.palette.Add(col);
            }
            io.Seek(SeekOrigin.Begin, colorPos);
            for (var i = 0; i < texture.width; i++)
            {
                for (var n = 0; n < texture.height; n++)
                {
                    texture.pixels.Add((int)io.ReadByte());
                }
            }
            textureAsset.engineTexture = Factories.Get.TextureFactory.CreateTexture(texture);
            return textureAsset;
        }
    }
}
