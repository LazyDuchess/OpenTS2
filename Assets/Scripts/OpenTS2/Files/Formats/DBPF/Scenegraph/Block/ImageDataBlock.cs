using System;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A Scenegraph cImageData block.
    ///
    /// Consists of a list of SubImage instances (usually only one) which have a MipMap for each of the sub images.
    /// </summary>
    public class ImageDataBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0x1C4A276C;
        public const string BLOCK_NAME = "cImageData";
        public override string BlockName => BLOCK_NAME;


        public int Width { get; }
        public int Height { get; }
        public ScenegraphTextureFormat ColorFormat { get; }
        public uint MipMapLevels { get; }
        public SubImage[] SubImages { get; }
        public uint SelectedImage { get; }

        public ImageDataBlock(PersistTypeInfo blockTypeInfo, int width, int height, ScenegraphTextureFormat colorFormat,
            uint mipMapLevels, SubImage[] subImages, uint selectedImage) : base(blockTypeInfo) =>
            (Width, Height, ColorFormat, MipMapLevels, SubImages, SelectedImage) =
            (width, height, colorFormat, mipMapLevels, subImages, selectedImage);
    }

    public readonly struct SubImage
    {
        public ImageMip[] MipMap { get; }
        public Color RepresentativeColor { get; }
        public float BumpScale { get; }

        public SubImage(ImageMip[] mipMap, Color representativeColor, float bumpScale) =>
            (MipMap, RepresentativeColor, BumpScale) = (mipMap, representativeColor, bumpScale);
    }

    /// <summary>
    /// Represents the data from a single mip-level of an image.
    ///
    /// Used to encapsulate the two different types these mips can be: LIFO references or raw data.
    /// </summary>
    public abstract class ImageMip
    {
    }

    public class LifoReferenceMip : ImageMip
    {
        public string LifoName { get; }
        public LifoReferenceMip(string lifoName) => (LifoName) = lifoName;
    }


    public class ByteArrayMip : ImageMip
    {
        public byte[] Data { get; }
        public ByteArrayMip(byte[] data) => (Data) = data;
    }

    public class ImageDataBlockReader : IScenegraphDataBlockReader<ImageDataBlock>
    {
        private static ImageMip[] ReadSubImageMipsVersion9AndOver(IoBuffer reader, uint mipLevels)
        {
            var subImageNumberOfMips = reader.ReadUInt32();
            Debug.Assert(subImageNumberOfMips <= mipLevels, "subImage has more mips than cImageData specified");

            var mipMap = new ImageMip[subImageNumberOfMips];
            for (var i = 0; i < subImageNumberOfMips; i++)
            {
                var isLifoReference = reader.ReadByte() != 0;

                if (isLifoReference)
                {
                    mipMap[i] = new LifoReferenceMip(reader.ReadVariableLengthPascalString());
                }
                else
                {
                    var dataSize = reader.ReadUInt32();
                    mipMap[i] = new ByteArrayMip(reader.ReadBytes(dataSize));
                }
            }

            return mipMap;
        }

        private static ImageMip[] ReadSubImageMipsVersion8AndBelow(IoBuffer reader, uint mipLevels)
        {
            var mipMap = new ImageMip[mipLevels];
            for (var i = 0; i < mipLevels; i++)
            {
                var dataSize = reader.ReadUInt32();
                mipMap[i] = new ByteArrayMip(reader.ReadBytes(dataSize));
            }

            return mipMap;
        }

        public ImageDataBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var resource = ScenegraphResource.Deserialize(reader);

            var width = reader.ReadInt32();
            var height = reader.ReadInt32();

            var colorFormatId = reader.ReadUInt32();
            var colorFormat = ScenegraphTextureFormatMethods.FromInt(colorFormatId);

            var mipMapLevels = reader.ReadUInt32();

            // Yup, this float is completely ignored!
            reader.ReadFloat();

            var subImageCount = reader.ReadUInt32();
            var selectedImage = reader.ReadUInt32();

            if (blockTypeInfo.Version > 6)
            {
                var imageName = reader.ReadVariableLengthPascalString();
            }

            var subImages = new SubImage[subImageCount];
            for (var i = 0; i < subImageCount; i++)
            {
                ImageMip[] mipMap = (blockTypeInfo.Version >= 9) switch
                {
                    true => ReadSubImageMipsVersion9AndOver(reader, mipMapLevels),
                    _ => ReadSubImageMipsVersion8AndBelow(reader, mipMapLevels)
                };

                // ARGB color value representative of the whole texture.
                Color representativeColor = (blockTypeInfo.Version > 6) switch
                {
                    true => new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                    _ => Color.white
                };

                var bumpScale = 10.0F;
                if (blockTypeInfo.Version > 7)
                {
                    bumpScale = reader.ReadFloat();
                }

                subImages[i] = new SubImage(mipMap, representativeColor, bumpScale);
            }

            return new ImageDataBlock(blockTypeInfo, width, height, colorFormat, mipMapLevels, subImages, selectedImage);
        }
    }

    public enum ScenegraphTextureFormat : uint
    {
        RGBA32 = 1,
        RGB24 = 2,
        Alpha8 = 3,
        DXT1 = 4,
        DXT3 = 5,

        // These are "luminance", a full white image with the bits
        // controlling transparency.
        Luminance8 = 6,
        Luminance16 = 7,
        DXT5 = 8,
        RGB24_repeat = 9, // Yup, two different values for the same format :/
    }

    internal static class ScenegraphTextureFormatMethods
    {
        public static ScenegraphTextureFormat FromInt(uint formatId)
        {
            if (!Enum.IsDefined(typeof(ScenegraphTextureFormat), formatId))
            {
                throw new Exception("Unknown cImageData color format: " + formatId);
            }

            return (ScenegraphTextureFormat)formatId;
        }
    }
}