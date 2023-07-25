using System;
using System.IO;
using NUnit.Framework;
using OpenTS2.Files.Formats.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Utils;
using AssertionException = UnityEngine.Assertions.AssertionException;

public class ScenegraphResourceCollectionTest
{
    [Test]
    public void DeserializationThrowsWhenHeaderIncorrect()
    {
        var wrongHeader = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        var reader = new IoBuffer(new MemoryStream(wrongHeader));

        var e = Assert.Throws<Exception>(() =>
        {
            ScenegraphResourceCollection.Deserialize(reader);
        });
        Assert.That(e.Message, Does.Contain("Scenegraph resource has invalid header"));
    }

    [Test]
    public void DeserializationThrowsWhenTypeNotSupported()
    {
        var writer = new BinaryWriter(new MemoryStream());
        // Header.
        writer.Write(0xFFFF0001);
        // Number of file links.
        writer.Write(0);
        // Number of data blocks.
        writer.Write(1);
        // Item types.
        writer.Write(0xbeef);
        // First item.
        new PersistTypeInfo("cFakeNode", 0xbeef, 1).Serialize(writer);

        var reader = new IoBuffer(writer.BaseStream);
        reader.ByteOrder = ByteOrder.LITTLE_ENDIAN;
        reader.Seek(SeekOrigin.Begin, 0);
        
        var e = Assert.Throws<NotImplementedException>(() =>
        {
            ScenegraphResourceCollection.Deserialize(reader);
        });
        Assert.That(e.Message, Does.Contain("Unimplemented Scenegraph type cFakeNode"));
    }

    [Test]
    public void DeserializesSingleItemSuccessfully()
    {
        var writer = new BinaryWriter(new MemoryStream());
        // Header.
        writer.Write(0xFFFF0001);
        // Number of file links.
        writer.Write(0);
        // Number of data blocks.
        writer.Write(1);
        // Item types.
        writer.Write(TagExtensionBlock.TYPE_ID);
        // First item.
        new PersistTypeInfo(TagExtensionBlock.BLOCK_NAME, TagExtensionBlock.TYPE_ID, 3).Serialize(writer);
        new PersistTypeInfo("cExtension", 0, 1).Serialize(writer);
        writer.Write("testTag");
        
        var reader = new IoBuffer(writer.BaseStream);
        reader.ByteOrder = ByteOrder.LITTLE_ENDIAN;
        reader.Seek(SeekOrigin.Begin, 0);

        var scenegraphCollection = ScenegraphResourceCollection.Deserialize(reader);
        Assert.That(scenegraphCollection.Blocks.Count, Is.EqualTo(1));
        
        var block = scenegraphCollection.Blocks[0];
        Assert.That(block, Is.TypeOf(typeof(TagExtensionBlock)));
        Assert.That(block.BlockTypeInfo.Name, Is.EqualTo(TagExtensionBlock.BLOCK_NAME));
        Assert.That(block.BlockTypeInfo.TypeId, Is.EqualTo(TagExtensionBlock.TYPE_ID));
        Assert.That(block.BlockTypeInfo.Version, Is.EqualTo(3));

        var extensionBlock = (TagExtensionBlock)block;
        Assert.That(extensionBlock.Tag, Is.EqualTo("testTag"));
    }

    [Test]
    public void DeserializesMultipleItemsSuccessfully()
    {
        var writer = new BinaryWriter(new MemoryStream());
        // Header.
        writer.Write(0xFFFF0001);
        // Number of file links.
        writer.Write(0);
        // Number of data blocks.
        writer.Write(2);
        // Item types.
        writer.Write(TagExtensionBlock.TYPE_ID);
        writer.Write(TagExtensionBlock.TYPE_ID);
        // First item.
        new PersistTypeInfo(TagExtensionBlock.BLOCK_NAME, TagExtensionBlock.TYPE_ID, 3).Serialize(writer);
        new PersistTypeInfo("cExtension", 0, 1).Serialize(writer);
        writer.Write("testTag1");
        // Second item.
        new PersistTypeInfo(TagExtensionBlock.BLOCK_NAME, TagExtensionBlock.TYPE_ID, 3).Serialize(writer);
        new PersistTypeInfo("cExtension", 0, 1).Serialize(writer);
        writer.Write("testTag2");
        
        var reader = new IoBuffer(writer.BaseStream);
        reader.ByteOrder = ByteOrder.LITTLE_ENDIAN;
        reader.Seek(SeekOrigin.Begin, 0);

        var scenegraphCollection = ScenegraphResourceCollection.Deserialize(reader);
        Assert.That(scenegraphCollection.Blocks.Count, Is.EqualTo(2));
        
        Assert.That(scenegraphCollection.Blocks[0], Is.TypeOf(typeof(TagExtensionBlock)));
        Assert.That(scenegraphCollection.Blocks[1], Is.TypeOf(typeof(TagExtensionBlock)));

        var block1 = (TagExtensionBlock)scenegraphCollection.Blocks[0];
        var block2 = (TagExtensionBlock)scenegraphCollection.Blocks[1];
        Assert.That(block1.Tag, Is.EqualTo("testTag1"));
        Assert.That(block2.Tag, Is.EqualTo("testTag2"));
    }
}