using System;
using System.IO;
using NUnit.Framework;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF.Scenegraph;
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
    public void TestLoadsTexture()
    {
        var textureAsset =
            ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey("brick_dxt1_txtr", 0x1C0532FA,
                0x1C4A276C));
        var texture = textureAsset.GetSelectedImageAsUnityTexture(ContentProvider.Get());

        Assert.That(texture.width, Is.EqualTo(128));
        Assert.That(texture.height, Is.EqualTo(128));
    }

    [Test]
    public void TestLoadedImageBlockHasCorrectDetails()
    {
        var textureAsset =
            ContentProvider.Get().GetAsset<ScenegraphTextureAsset>(new ResourceKey("brick_dxt1_txtr", 0x1C0532FA,
                0x1C4A276C));
        var imageBlock = textureAsset.ImageDataBlock;
        
        Assert.That(imageBlock.BlockName, Is.EqualTo("cImageData"));
        Assert.That(imageBlock.SubImages.Length, Is.EqualTo(1));
        Assert.That(imageBlock.SelectedImage, Is.EqualTo(0));
        Assert.That(imageBlock.ColorFormat, Is.EqualTo(ScenegraphTextureFormat.DXT1));
        Assert.That(imageBlock.SubImages[0].MipMap.Length, Is.EqualTo(8));
    }
}