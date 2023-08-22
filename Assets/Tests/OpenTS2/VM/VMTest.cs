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
using OpenTS2.SimAntics.Primitives;

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
        var bhav = ContentProvider.Get().GetAsset<BHAVAsset>(new ResourceKey(0x1001, _groupID, TypeIDs.BHAV));

        var vm = new VM();
        var entity = new VMEntity(vm);
        vm.AddEntity(entity);

        var stackFrame = new VMStackFrame(bhav, entity.Stack);
        entity.Stack.Frames.Push(stackFrame);

        // The test BHAV multiplies Param0 by 2, stores it in Temp0, sleeps for a single tick, then sets Temp0 to 1200
        stackFrame.Arguments[0] = 10;

        vm.Tick();
        Assert.That(entity.Temps[0], Is.EqualTo(20));
        vm.Tick();
        Assert.That(entity.Temps[0], Is.EqualTo(1200));
    }

    [Test]
    public void TestPrimitiveRegistry()
    {
        var vmExpressionPrim = VMPrimitiveRegistry.GetPrimitive(0x2);
        Assert.That(vmExpressionPrim, Is.TypeOf(typeof(VMExpression)));
    }
}
