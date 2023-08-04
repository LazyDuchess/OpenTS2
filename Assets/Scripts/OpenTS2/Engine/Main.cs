using OpenTS2.Assemblies;
using OpenTS2.Client;
using OpenTS2.Content;
using OpenTS2.Diagnostic;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Rendering;
using UnityEngine;

namespace OpenTS2.Engine
{
    /// <summary>
    /// Main initialization class for Unity Engine implementation of OpenTS2.
    /// </summary>
    public static class Main
    {
        static bool s_initialized = false;
        /// <summary>
        /// Initializes all singletons, systems and the game assembly.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (s_initialized)
                return;
            var settings = new Settings();
            var epManager = new EPManager();
            var contentProvider = new ContentProvider();
            var objectManager = new ObjectManager(contentProvider);
            var effectsManager = new EffectsManager(contentProvider);

            TerrainManager.Initialize();
            MaterialManager.Initialize();
            Filesystem.Initialize(new JSONPathProvider(), epManager);
            CodecAttribute.Initialize();
            CheatSystem.Initialize();
            //Initialize the game assembly, do all reflection things.
            AssemblyHelper.InitializeLoadedAssemblies();
            s_initialized = true;
        }
    }
}
