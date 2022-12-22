using NUnit.Framework;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Common;
using System.IO;
using UnityEngine;
using System.Text;

public class DPBFFileTest
{
    [Test]
    public void DBPFFileSaveLoadTest()
    {
        var dummyTGI = new ResourceKey("dummyresource", GroupIDs.Local, 0x0);
        var dummyResourceBytes = Encoding.ASCII.GetBytes("TestResource");
        var packagePath = "TestFiles/TestPackage.package";

        var package = new DBPFFile();
        package.FilePath = packagePath;
        Assert.IsTrue(package.Empty);
        package.Changes.Set(dummyResourceBytes, dummyTGI, false);
        Assert.IsTrue(package.Entries.Count == 1);
        var data = package.GetItemByTGI(dummyTGI);
        Assert.IsTrue(data == dummyResourceBytes);
        package.WriteToFile();
        package.Dispose();

        var loadedPackage = new DBPFFile(packagePath);
        Assert.IsTrue(package.Entries.Count == 1);
        var dummyResource = loadedPackage.GetItemByTGI(dummyTGI);
        Assert.IsTrue(Encoding.ASCII.GetString(dummyResource) == Encoding.ASCII.GetString(dummyResourceBytes));
        loadedPackage.Dispose();

        if (File.Exists(packagePath))
            File.Delete(packagePath);
    }
}
