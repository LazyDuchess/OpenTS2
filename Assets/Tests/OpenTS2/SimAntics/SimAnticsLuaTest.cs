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
using OpenTS2.Content.DBPF;

public class SimAnticsLuaTest
{
    private uint _groupID;

    [SetUp]
    public void SetUp()
    {
        TestMain.Initialize();
        _groupID = ContentProvider.Get().AddPackage("TestAssets/SimAntics/lua.package").GroupID;
    }

    [Test]
    public void TestRunLua()
    {
        // VM Entities need to be attached to an OBJD to be aware of private/semiglobal scope.
        var testObjectDefinition = new ObjectDefinitionAsset();
        testObjectDefinition.TGI = new ResourceKey(1, _groupID, TypeIDs.OBJD);

        var bhav = VM.GetBHAV(0x1001, _groupID);

        var vm = new VM();
        var entity = new VMEntity(testObjectDefinition);
        vm.AddEntity(entity);

        var stackFrame = new VMStackFrame(bhav, entity.MainThread);
        entity.MainThread.Frames.Push(stackFrame);

        // Lua script multiplies 2 by 3.
        vm.Tick();
        Assert.That(entity.Temps[0], Is.EqualTo(6));
    }
}
