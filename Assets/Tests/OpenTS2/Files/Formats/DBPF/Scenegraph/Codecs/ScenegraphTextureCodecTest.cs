using System;
using System.IO;
using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using UnityEngine;

public class ScenegraphTextureCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        ContentProvider.Get().AddPackage("TestAssets/Scenegraph/textures.package");
    }

    [Test]
    public void TestLoadsTexturesWithNoLifo()
    {
        var textureAsset =
            ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey("brick_dxt1_no_lifo_txtr", 0x1C0532FA,
                TypeIDs.SCENEGRAPH_TXTR));
        var texture = textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get());

        Assert.That(texture.width, Is.EqualTo(128));
        Assert.That(texture.height, Is.EqualTo(128));
    }
    
    [Test]
    public void TestLoadedImageBlockWithNoLifoHasCorrectDetails()
    {
        var textureAsset =
            ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey("brick_dxt1_no_lifo_txtr", 0x1C0532FA,
                TypeIDs.SCENEGRAPH_TXTR));
        var imageBlock = textureAsset.ImageDataBlock;
        
        Assert.That(imageBlock.BlockName, Is.EqualTo("cImageData"));
        Assert.That(imageBlock.ColorFormat, Is.EqualTo(ScenegraphTextureFormat.DXT1));
        Assert.That(imageBlock.Width, Is.EqualTo(128));
        Assert.That(imageBlock.Height, Is.EqualTo(128));

        Assert.That(imageBlock.SubImages.Length, Is.EqualTo(1));
        Assert.That(imageBlock.SelectedImage, Is.EqualTo(0));

        Assert.That(imageBlock.SubImages[0].MipMap.Length, Is.EqualTo(8));
        Assert.That(imageBlock.SubImages[0].MipMap[0], Is.InstanceOf<ByteArrayMip>());
        Assert.That(imageBlock.SubImages[0].MipMap[7], Is.InstanceOf<ByteArrayMip>());
    }

    [Test]
    public void TestLoadsTextureWithLifo()
    {
        var textureAsset =
            ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey("brick_dxt1_txtr", 0x1C0532FA,
                TypeIDs.SCENEGRAPH_TXTR));
        var texture = textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get());

        Assert.That(texture.width, Is.EqualTo(256));
        Assert.That(texture.height, Is.EqualTo(256));
    }

    [Test]
    public void TestLoadedImageBlockWithLifoHasCorrectDetails()
    {
        var textureAsset =
            ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey("brick_dxt1_txtr", 0x1C0532FA,
                TypeIDs.SCENEGRAPH_TXTR));
        var imageBlock = textureAsset.ImageDataBlock;
        
        Assert.That(imageBlock.BlockName, Is.EqualTo("cImageData"));
        Assert.That(imageBlock.ColorFormat, Is.EqualTo(ScenegraphTextureFormat.DXT1));
        Assert.That(imageBlock.Width, Is.EqualTo(256));
        Assert.That(imageBlock.Height, Is.EqualTo(256));

        Assert.That(imageBlock.SubImages.Length, Is.EqualTo(1));
        Assert.That(imageBlock.SelectedImage, Is.EqualTo(0));

        Assert.That(imageBlock.SubImages[0].MipMap.Length, Is.EqualTo(9));
        // Check that the first mip just contains image data.
        Assert.That(imageBlock.SubImages[0].MipMap[0], Is.InstanceOf<ByteArrayMip>());
        // Check that the last mip is a LIFO reference.
        Assert.That(imageBlock.SubImages[0].MipMap[8], Is.InstanceOf<LifoReferenceMip>());
    }

    [Test]
    public void TestLoadsTextureThatSqueezesToOnePixel()
    {
        // This test is for an edge case of resolutions like 32x16 with 6 mip levels. If we naively go down the path
        // of dividing the shorter side by 2 for each mip level we end up with:
        //
        // mip level:  6 -> 5 -> 4 -> 3 -> 2 -> 1
        // pixels:    16 -> 8 -> 4 -> 2 -> 1 -> 0
        //
        // but we can't have a 1x0 resolution texture, so gotta make sure we clamp that lower value to at least 1
        // pixel :)
        var textureAsset =
            ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey("small_non_square_txtr", 0x1C0532FA,
                TypeIDs.SCENEGRAPH_TXTR));

        // Check that the smallest mip is a 1x1 image.
        var texture = textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get());
        Assert.That(texture.GetPixels32(5).Length, Is.EqualTo(1));
    }

    [Test]
    public void TestLoadsSixteenBySixtyFourDxt3Texture()
    {
        // DXT3 blocks are 4x4 but in this situation we end up with a width of 2 and height of 8. Make sure our block
        // iteration loop can handle that.
        var textureAsset = ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey("neighborhood-stopsign_txtr", 0x1C0532FA,
            TypeIDs.SCENEGRAPH_TXTR));

        var texture = textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get());
        Assert.That(texture.GetPixels32(6).Length, Is.EqualTo(1));
    }
}