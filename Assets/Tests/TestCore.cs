using OpenTS2.Assemblies;
using OpenTS2.Content;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Lua;
using OpenTS2.SimAntics.Primitives;
using UnityEngine;

/// <summary>
/// Main initialization class for OpenTS2 unit testing.
/// </summary>
public static class TestCore
{
    /// <summary>
    /// Initializes all singletons, systems and the game assembly.
    /// </summary>
    public static void Initialize()
    {
        var globals = new GameGlobals();
        GameGlobals.allowCustomContent = false;
        var epManager = new EPManager((int)ProductFlags.BaseGame);
        var contentManager = new ContentManager();
        var luaManager = new LuaManager();

        Filesystem.Initialize(new TestPathManager(), epManager);
        CodecAttribute.Initialize();
        AssemblyHelper.InitializeLoadedAssemblies();
        VMPrimitiveRegistry.Initialize();
    }
}