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
        public static List<AbstractPlugin> LoadedPlugins = new List<AbstractPlugin>();
        /// <summary>
        /// Initializes all plugin assemblies it can find in loaded packages.
        /// </summary>
        public static void Initialize()
        {
            var contentProvider = ContentProvider.Get();
            var assemblyAssets = contentProvider.GetAssetsOfType<AssemblyAsset>(TypeIDs.DLL);
            var loadedAssemblies = new List<Assembly>();
            foreach(var element in assemblyAssets)
            {
                var modAssembly = Assembly.Load(element.Data);
                loadedAssemblies.Add(modAssembly);
            }
            foreach(var element in loadedAssemblies)
            {
                AssemblyHelper.InitializeAssembly(element);
                var types = element.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(AbstractPlugin).IsAssignableFrom(type))
                    {
                        var pluginInstance = Activator.CreateInstance(type) as AbstractPlugin;
                        pluginInstance.Assembly = element;
                        LoadedPlugins.Add(pluginInstance);
                        break;
                    }
                }
            }
        }
    }
}
