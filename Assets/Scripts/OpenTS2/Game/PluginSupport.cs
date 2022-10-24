using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System.Reflection;
using OpenTS2.Assemblies;

namespace OpenTS2.Game
{
    public static class PluginSupport
    {
        public static List<AbstractPlugin> loadedPlugins = new List<AbstractPlugin>();
        /// <summary>
        /// Initializes all plugin assemblies it can find in loaded packages.
        /// </summary>
        public static void Initialize()
        {
            var contentManager = ContentManager.Get();
            var assemblyAssets = contentManager.Provider.GetAssetsOfType<AssemblyAsset>(TypeIDs.DLL);
            foreach(var element in assemblyAssets)
            {
                var modAssembly = Assembly.Load(element.data);
                AssemblyHelper.InitializeAssembly(modAssembly);
                var types = modAssembly.GetTypes();
                foreach(var type in types)
                {
                    if (typeof(AbstractPlugin).IsAssignableFrom(type))
                    {
                        var pluginInstance = Activator.CreateInstance(type) as AbstractPlugin;
                        pluginInstance.assembly = modAssembly;
                        loadedPlugins.Add(pluginInstance);
                        break;
                    }
                }
            }
        }
    }
}
