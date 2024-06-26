﻿using System.Linq;
using NUnit.Framework;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;

public class NeighborhoodObjectCodecTest
{
    [SetUp]
    public void SetUp()
    {
        TestCore.Initialize();
        ContentManager.Instance.AddPackage("TestAssets/Codecs/NeighborhoodDecorations.package");
    }

    [Test]
    public void TestSuccessfullyLoadsNeighborhoodObject()
    {
        var objectAsset = ContentManager.Instance
            .GetAssetsOfType<NeighborhoodObjectAsset>(TypeIDs.NHOOD_OBJECT).Single();

        Assert.That(objectAsset.ModelName, Is.EqualTo("ufoCrash_cres"));
        Assert.That(objectAsset.Guid, Is.EqualTo(0x16));
    }
}