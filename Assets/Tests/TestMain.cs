using OpenTS2.Assemblies;
using OpenTS2.Client;
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
            Shutdown();
        var settings = new Settings()
        {
            CustomContentEnabled = false,
            Language = Languages.USEnglish
        };
        var epManager = new EPManager((int)ProductFlags.BaseGame);
        var contentProvider = new ContentProvider();
        Filesystem.Initialize(new TestPathProvider(), epManager);
        CodecAttribute.Initialize();
        AssemblyHelper.InitializeLoadedAssemblies();
        s_initialized = true;
    }

    public static void Shutdown()
    {
        s_initialized = false;
    }
}