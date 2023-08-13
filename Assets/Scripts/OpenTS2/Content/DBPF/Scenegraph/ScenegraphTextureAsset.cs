using System;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Scenegraph
{
    public class ScenegraphTextureAsset : AbstractAsset
    {
        public ImageDataBlock ImageDataBlock { get; }

        public ScenegraphTextureAsset(ImageDataBlock imageDataBlock)
        {
            ImageDataBlock = imageDataBlock;
        }

        private Texture2D _texture;

        public override void FreeUnmanagedResources()
        {
            if (_texture == null)
                return;
            _texture.Free();
        }

        // TODO: Maybe we should just use `ContentProvider.Get` and compute this eagerly instead of having a
        //       ContentProvider passed in here. That way we could also drop having to store the full ImageDataBlock
        //       and just have the more compact Texture2D.
        public Texture2D GetSelectedImageAsUnityTexture(ContentProvider provider)
        {
            if (_texture != null)
            {
                return _texture;
            }

            var subImage = ImageDataBlock.SubImages[ImageDataBlock.SelectedImage];
            _texture = SubImageToTexture(provider, ImageDataBlock.ColorFormat, ImageDataBlock.Width, ImageDataBlock.Height, subImage);
            return _texture;
        }

        /// <summary>
        /// Compute the full Texture2D with mipmaps using the SubImage data.
        /// </summary>
        public static Texture2D SubImageToTexture(ContentProvider provider, ScenegraphTextureFormat colorFormat, int width, int height, SubImage subImage)
        {
            var format = ScenegraphTextureFormatToUnity(colorFormat);
            var texture = new Texture2D(width, height, format, mipChain: true);

            var currentMipLevel = 0;
            for (int i = subImage.MipMap.Length - 1; i >= 0; i--)
            {
                var mip = subImage.MipMap[i];
                byte[] mipData;
                switch (mip)
                {
                    case LifoReferenceMip lifoReferenceMip:
                    {
                        var lifoAsset = provider.GetAsset<ScenegraphMipLevelInfoAsset>(
                            new ResourceKey(lifoReferenceMip.LifoName, GroupIDs.Scenegraph, TypeIDs.SCENEGRAPH_LIFO));
                        mipData = lifoAsset.MipData;
                        break;
                    }
                    case ByteArrayMip byteArrayMip:
                        mipData = byteArrayMip.Data;
                        break;
                    default:
                        throw new ArgumentException($"SubImage has invalid mip type: {mip}");
                }

                var pixelData = ConvertPixelDataForUnity(colorFormat, mipData, width, height);
                texture.SetPixelData(pixelData, currentMipLevel);

                currentMipLevel++;
                // Make sure the width and height are always at least 1-pixel.
                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);
            }
            texture.Apply();
            return texture;
        }

        private static byte[] ConvertPixelDataForUnity(ScenegraphTextureFormat format, byte[] data, int width,
            int height)
        {
            switch (format)
            {
                // All of these can be used as-is without conversion.
                case ScenegraphTextureFormat.RGBA32:
                case ScenegraphTextureFormat.RGB24:
                case ScenegraphTextureFormat.Alpha8:
                case ScenegraphTextureFormat.DXT1:
                case ScenegraphTextureFormat.Luminance8:
                case ScenegraphTextureFormat.Luminance16:
                case ScenegraphTextureFormat.DXT5:
                case ScenegraphTextureFormat.RGB24_repeat:
                    return data;
                // This needs a conversion as unity no longer supports DXT-3 natively.
                case ScenegraphTextureFormat.DXT3:
                    return ConvertDxt3ToRgba(data, width, height);
                default:
                    throw new NotImplementedException($"Cannot convert texture of type {format} for unity");
            }
        }

        private static TextureFormat ScenegraphTextureFormatToUnity(ScenegraphTextureFormat format)
        {
            var unityFormat = format switch
            {
                ScenegraphTextureFormat.RGBA32 => TextureFormat.BGRA32,
                // TODO: this might be wrong as the game uses BRGA for 32-bit textures.
                ScenegraphTextureFormat.RGB24 => TextureFormat.RGB24,
                ScenegraphTextureFormat.Alpha8 => TextureFormat.Alpha8,
                ScenegraphTextureFormat.DXT1 => TextureFormat.DXT1,
                // Note, DXT3 is converted to RGBA.
                ScenegraphTextureFormat.DXT3 => TextureFormat.RGBA32,
                ScenegraphTextureFormat.Luminance8 => TextureFormat.R8,
                ScenegraphTextureFormat.Luminance16 => TextureFormat.R16,
                ScenegraphTextureFormat.DXT5 => TextureFormat.DXT5,
                // TODO: this might be wrong as the game uses BRGA for 32-bit textures.
                ScenegraphTextureFormat.RGB24_repeat => TextureFormat.RGB24,
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
            return unityFormat;
        }

        private static byte[] ConvertDxt3ToRgba(byte[] data, int width, int height)
        {
            // TODO: Should we bother optimizing this? Currently we let unity load the texture as DXT5, convert it to
            //       RGBA32 and fix the alpha values. The only difference in DXT3 and DXT5 is how alpha values are
            //       encoded. We can probably change this to an optimized C/C++ implementation if this
            //       really is a bottleneck.
            // Will contain 4-bit alpha values for each pixel. We take a max of 16 here because a DXT image must
            // contain at least one 4x4 block.
            var alphaValues = new byte[Math.Max(width * height, 16)];

            // Iterate in 4x4 blocks of encoded pixels.
            // Each of the blocks contains 16 bytes of color and alpha data.
            for (var i = 0; i < data.Length / 16; i++)
            {
                var blockX = i % Math.Max(width / 4, 1);
                var blockY = i / Math.Max(width / 4, 1);
                var outputIndex = blockX * 4 + (blockY * width * 4);
                var inputBlockDataIndex = i * 16;

                // DXT3 stores 16 4-bit alpha values starting at `i`.
                for (var j = 0; j < Math.Min(4, width); j++)
                {
                    for (var k = 0; k < Math.Min(4, height); k++)
                    {
                        var pixelIndex = (j * 4) + k;
                        var outputAlphaIndex = outputIndex + (j * width) + k;
                        // This can happen with really narrow images like 8x2.
                        if (outputAlphaIndex >= alphaValues.Length)
                        {
                            continue;
                        }
                        // Get the lower 4-bits if we're at pixels 0, 2, 4 etc or the higher 4-bits for even pixels.
                        alphaValues[outputAlphaIndex] = (pixelIndex % 2) switch
                        {
                            0 => (byte)(data[inputBlockDataIndex + (pixelIndex / 2)] & 0xF),
                            _ => (byte)((data[inputBlockDataIndex + (pixelIndex / 2)] >> 4) & 0xF)
                        };
                    }
                }
            }

            var asTexture = new Texture2D(width, height, TextureFormat.DXT5, mipChain: false);
            asTexture.SetPixelData(data, mipLevel: 0);
            asTexture.Apply();

            var pixels = asTexture.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i].a = alphaValues[i] / (float)0xF;
            }

            var asRgbaTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            asRgbaTexture.SetPixels(pixels);
            asRgbaTexture.Apply();
            return asRgbaTexture.GetRawTextureData();
        }
    }
}