using System.IO;
using NUnit.Framework;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

public class ScenegraphTextureAssetTest
{
    // This is the only format we test explicitly because we handle the conversion. Rest are assumed to be loaded
    // properly by unity.
    [Test]
    public void TestSuccessfullyConvertsSquareDxt3Image()
    {
        // Skip a bunch of details of reading DDS files here, skip straight to the image data.
        // Our image is 128x128.
        var reader = new BinaryReader(new FileStream( "TestAssets/Scenegraph/brick-texture-dxt3.dds", FileMode.Open));
        Assert.That(reader.ReadChars(4), Is.EqualTo("DDS "));
        reader.BaseStream.Seek(124, SeekOrigin.Current);
        // DXT3 encodes pixels into 4x4 blocks each containing 16 bytes of alpha/color data.
        var imageData = reader.ReadBytes((128 / 4) * (128 / 4) * 16);
        
        var mip = new ImageMip[] { new ByteArrayMip(imageData) };
        var subImage = new SubImage(mip, Color.white, 1.0f);
        
        var toTexture = ScenegraphTextureAsset.SubImageToTexture(ContentProvider.Get(), ScenegraphTextureFormat.DXT3,
            128, 128, subImage);

        var actualImage = new Texture2D(128, 128, TextureFormat.RGBA32, mipChain:false);
        actualImage.LoadImage(File.ReadAllBytes("TestAssets/Scenegraph/brick-texture.png"));
        
        Assert.That(toTexture.GetPixels(), Is.EqualTo(actualImage.GetPixels()));
    }

    [Test]
    public void TestSuccessfullyConvertsNonSquareDxt3Image()
    {
        // Skip a bunch of details of reading DDS files here, skip straight to the image data.
        // Our image is 256x128.
        var reader = new BinaryReader(new FileStream( "TestAssets/Scenegraph/cc0-logo-dxt3.dds", FileMode.Open));
        Assert.That(reader.ReadChars(4), Is.EqualTo("DDS "));
        reader.BaseStream.Seek(124, SeekOrigin.Current);
        var imageData = reader.ReadBytes((256 / 4) * (128 / 4) * 16);
        
        var mip = new ImageMip[] { new ByteArrayMip(imageData) };
        var subImage = new SubImage(mip, Color.white, 1.0f);
        
        var toTexture = ScenegraphTextureAsset.SubImageToTexture(ContentProvider.Get(), ScenegraphTextureFormat.DXT3,
            256, 128, subImage);
        
        var actualImage = new Texture2D(128, 128, TextureFormat.RGBA32, mipChain:false);
        actualImage.LoadImage(File.ReadAllBytes("TestAssets/Scenegraph/cc0-logo.png"));
        
        Assert.That(toTexture.GetPixels(), Is.EqualTo(actualImage.GetPixels()));
    }
}