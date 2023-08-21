using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenTS2.Content;
using OpenTS2.SimAntics;
using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF;

public class VMTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        _groupID = ContentProvider.Get().AddPackage("TestAssets/VM/bhav.package").GroupID;
    }

    [Test]
    public void TestLoadsBHAV()
    {
        var bhav = ContentProvider.Get().GetAsset<BHAVAsset>(new ResourceKey(0x1001, _groupID, TypeIDs.BHAV));

        Assert.That(bhav.FileName, Is.EqualTo("OpenTS2 BHAV Test"));
        Assert.That(bhav.ArgumentCount, Is.EqualTo(1));
        Assert.That(bhav.LocalCount, Is.EqualTo(0));
    }

    [Test]
    public void TestRunBHAV()
    {
        var vm = new VM();
        var entity = new VMEntity();
        entity.ID = 1;
        vm.Entities.Add(entity);
        var bhav = ContentProvider.Get().GetAsset<BHAVAsset>(new ResourceKey(0x1001, _groupID, TypeIDs.BHAV));
    }
}
