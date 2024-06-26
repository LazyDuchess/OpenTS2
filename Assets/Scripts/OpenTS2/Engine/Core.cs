using OpenTS2.Assemblies;
using OpenTS2.Content;
using OpenTS2.Diagnostic;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Files;
using OpenTS2.Lua;
using OpenTS2.Rendering;
using OpenTS2.SimAntics.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using OpenTS2.Client;
using OpenTS2.Game;

namespace OpenTS2.Engine
{
    public class Core : MonoBehaviour
    {
        public string TargetScene;
        public static Action OnBeginLoading;
        public static Action OnFinishedLoading;
        public static Action OnNeighborhoodEntered;
        public static bool CoreInitialized = false;

        public static void InitializeCore()
        {
            if (CoreInitialized) return;

            var settings = new Settings();
            var epManager = new EPManager();
            var contentManager = new ContentManager();
            var effectsManager = new EffectsManager();
            var catalogManager = new CatalogManager();
            var luaManager = new LuaManager();

            TerrainManager.Initialize();
            MaterialManager.Initialize();
            Filesystem.Initialize(new JSONPathManager(), epManager);
            CodecAttribute.Initialize();
            CheatSystem.Initialize();
            VMPrimitiveRegistry.Initialize();
            //Initialize the game assembly, do all reflection things.
            AssemblyHelper.InitializeLoadedAssemblies();
            PluginSupport.Initialize();
            CoreInitialized = true;
        }

        private void Awake()
        {
            OnFinishedLoading += NeighborhoodManager.Initialize;

            DontDestroyOnLoad(gameObject);
            if (!string.IsNullOrEmpty(TargetScene))
                SceneManager.LoadScene(TargetScene);
        }
    }
}
