using System.IO;
using NUnit.Framework;
using OpenTS2.Files.Formats.DBPF.Scenegraph;
using OpenTS2.Files.Utils;

public class PersistTypeInfoTest
{
    [Test]
    public void DeserializesFromKnownData()
    {
        var cImageDataTypeInfo = new byte[]
        {
            0x0A, 0x63, 0x49, 0x6D, 0x61, 0x67, 0x65, 0x44, 0x61, 0x74, 0x61, 0x6C, 0x27, 0x4A, 0x1C, 0x09, 0x00, 0x00,
            0x00
        };

        var reader = new IoBuffer(new MemoryStream(cImageDataTypeInfo));
        reader.ByteOrder = ByteOrder.LITTLE_ENDIAN;

        var typeInfo = PersistTypeInfo.Deserialize(reader);
        Assert.That(typeInfo.Name, Is.EqualTo("cImageData"));
        Assert.That(typeInfo.TypeId, Is.EqualTo(0x1C4A276C));
        Assert.That(typeInfo.Version, Is.EqualTo(9));
    }

    [Test]
    public void TypeInfoRoundTripsSuccessfully()
    {
        var typeInfo = new PersistTypeInfo("cFakeNode", 0xbeef, 1);

        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        typeInfo.Serialize(writer);

        var reader = new IoBuffer(stream);
        reader.ByteOrder = ByteOrder.LITTLE_ENDIAN;
        reader.Seek(SeekOrigin.Begin, 0);

        var deserializedTypeInfo = PersistTypeInfo.Deserialize(reader);
        Assert.That(deserializedTypeInfo, Is.EqualTo(typeInfo));
    }
}