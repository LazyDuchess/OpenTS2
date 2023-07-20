using System;
using NUnit.Framework;
using OpenTS2.Files.Formats.XML;

public class PropertySetTest
{
    // Note: the double double-quotes in these tests are because we use the C#
    // @"string" feature.
    [Test]
    public void ParsesPropertySetSuccessfully()
    {
        const string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<cGZPropertySetString>
  <AnyUint32 key=""key-with-int"" type=""0xeb61e4f7"">42</AnyUint32>
  <AnyString key=""key-with-string"" type=""0x0b8bea18"">hello world</AnyString>
</cGZPropertySetString>";
        var set = new PropertySet(xml);

        Assert.That(set.Properties, Contains.Key("key-with-int"));
        Assert.That(set.Properties, Contains.Key("key-with-string"));
        Assert.That(set.GetProperty<StringProp>("key-with-string").Value, Is.EqualTo("hello world"));
        Assert.That(set.GetProperty<Uint32Prop>("key-with-int").Value, Is.EqualTo(42));
    }

    [Test]
    public void GetPropertyThrowsWhenTypeDoesNotMatch()
    {
        const string xml = @"
<cGZPropertySetString>
  <AnyUint32 key=""key-with-int"" type=""0xeb61e4f7"">42</AnyUint32>
</cGZPropertySetString>";
        var set = new PropertySet(xml);

        Assert.That(set.Properties, Contains.Key("key-with-int"));
        Assert.Throws<ArgumentException>(() => set.GetProperty<StringProp>("key-with-int"));
    }

    [Test]
    public void ParserFailsWhenXmlDoesNotHaveCorrectElement()
    {
        const string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<oops>
</oops>";

        Assert.Throws<ArgumentException>(() => new PropertySet(xml));
    }

    [Test]
    public void ParseFailsWhenPropertyDoesNotHaveKey()
    {
        const string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<cGZPropertySetString>
  <AnyUint32 type=""0xeb61e4f7"">42</AnyUint32>
</cGZPropertySetString>";

        Assert.Throws<ArgumentException>(() => new PropertySet(xml));
    }
}