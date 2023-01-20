using NUnit.Framework;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Common;
using System.IO;
using System.Text;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;

public class DPBFFileTest
{
    /// <summary>
    /// Creates a new package and adds a dummy resource to it.
    /// Then loads the package again and verifies that the resource is present and everything matches as it should.
    /// </summary>
    [Test]
    public void DBPFFileSaveLoadTest()
    {
        var dummyTGI = new ResourceKey("dummyresource", GroupIDs.Local, 0x0);
        var dummyResourceBytes = Encoding.ASCII.GetBytes("TestResource");
        var packagePath = "TestFiles/TestPackage.package";

        // Create new package
        var package = new DBPFFile();
        package.FilePath = packagePath;

        // Should be empty as we just created it.
        Assert.IsTrue(package.Empty);

        // Add the dummy resource to it.
        package.Changes.Set(dummyResourceBytes, dummyTGI, false);

        // Should have 1 entry now.
        Assert.IsTrue(package.Entries.Count == 1);

        // Test that the data returned matches.
        var data = package.GetBytesByTGI(dummyTGI);
        Assert.IsTrue(Encoding.ASCII.GetString(dummyResourceBytes) == Encoding.ASCII.GetString(data));

        // Save the package to a file and dispose of it.
        package.WriteToFile();
        package.Dispose();

        // Reload the package we just saved.
        var loadedPackage = new DBPFFile(packagePath);

        // Should have 1 entry.
        Assert.IsTrue(package.Entries.Count == 1);

        // Test that the entry matches with what we wrote earlier.
        var dummyResource = loadedPackage.GetBytesByTGI(dummyTGI);
        Assert.IsTrue(Encoding.ASCII.GetString(dummyResource) == Encoding.ASCII.GetString(dummyResourceBytes));

        // Dispose of the package and delete the file.
        loadedPackage.Dispose();

        if (File.Exists(packagePath))
            File.Delete(packagePath);
    }

    [Test]
    public void DBPPFFileSaveCompressedEntryTest()
    {
        TestMain.Initialize();
        var packageSavePath = "TestFiles/TestPackage.package";
        var packageLoadPath = "TestAssets/TestPackage.package";
        var package = new DBPFFile(packageLoadPath);
        var stringEntry = package.GetEntryByTGI(new ResourceKey(1, GroupIDs.Local, TypeIDs.STR));
        Assert.IsNotNull(stringEntry);

        // Set compression to true for entry.
        package.Changes.SetCompressed(stringEntry, true);
        package.FilePath = packageSavePath;

        // Save the package to a file and dispose of it.
        package.WriteToFile();
        package.Dispose();

        // Reload the package we just saved.
        var loadedPackage = new DBPFFile(packageSavePath);

        // Should have 2 entries (compression directory)
        Assert.IsTrue(package.Entries.Count == 2);

        var stringEntryLoaded = loadedPackage.GetEntryByTGI(new ResourceKey(1, GroupIDs.Local, TypeIDs.STR));
        
        // Make sure the entry is compressed.
        Assert.IsTrue(loadedPackage.IsCompressed(stringEntryLoaded));

        // Dispose of the package and delete the file.
        loadedPackage.Dispose();

        if (File.Exists(packageSavePath))
            File.Delete(packageSavePath);
    }
}
