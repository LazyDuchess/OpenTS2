using NUnit.Framework;
using OpenTS2.Content.DBPF;
using OpenTS2.Client;
using System.Collections.Generic;

public class StringSetTest
{
    [Test]
    public void StringSetLocalizationTest()
    {
        // Create StringSet Resource.
        var usString = "US String";
        var ukString = "UK String";
        var stringSetData = new StringSetData();
        stringSetData.FileName = "StringSetTest";
        stringSetData.Strings[Languages.USEnglish] = new List<StringValue>();
        stringSetData.Strings[Languages.USEnglish].Add(new StringValue(usString));
        var stringSet = new StringSetAsset(stringSetData);

        // Test US Localization String.
        Assert.AreEqual(usString, stringSet.StringData.GetString(0, Languages.USEnglish));

        // Test fallback to US String when UK Language (or any other) is not present.
        Assert.AreEqual(usString, stringSet.StringData.GetString(0, Languages.UKEnglish));

        // Add UK string to StringSet.
        stringSet.StringData.Strings[Languages.UKEnglish] = new List<StringValue>();
        stringSet.StringData.Strings[Languages.UKEnglish].Add(new StringValue(ukString));

        // Test that the UK string is now returned.
        Assert.AreEqual(ukString, stringSet.StringData.GetString(0, Languages.UKEnglish));
    }
}
