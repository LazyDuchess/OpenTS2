using OpenTS2.Assemblies;
using OpenTS2.Content;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;

/// <summary>
/// Main initialization class for OpenTS2 unit testing.
/// </summary>
public static class TestMain
{
    static bool s_initialized = false;
    /// <summary>
    /// Initializes all singletons, systems and the game assembly.
    /// </summary>
    public static void Initialize()
    {
        if (s_initialized)
            return;
        var epManager = new EPManager((int)ProductFlags.BaseGame);
        var contentManager = new ContentManager();
        Filesystem.Initialize(new TestPathProvider(), epManager);
        CodecAttribute.Initialize();
        AssemblyHelper.InitializeLoadedAssemblies();
        s_initialized = true;
    }
}