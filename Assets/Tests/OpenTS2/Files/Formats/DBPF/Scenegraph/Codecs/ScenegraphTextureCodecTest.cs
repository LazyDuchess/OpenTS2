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
}