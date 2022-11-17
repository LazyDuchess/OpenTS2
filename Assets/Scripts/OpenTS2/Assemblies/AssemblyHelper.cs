using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using OpenTS2.Files.Formats.DBPF;

namespace OpenTS2.Assemblies
{
    public static class AssemblyHelper
    {
        public static Action<Type, Assembly> AssemblyProcesses;

        public static void InitializeLoadedAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var element in assemblies)
            {
                InitializeAssembly(element);
            }
        }

        /// <summary>
        /// Initializes an assembly, does Reflection tasks like parsing attributes.
        /// </summary>
        /// <param name="assembly"></param>
        public static void InitializeAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                AssemblyProcesses?.Invoke(type, assembly);
            }
        }
    }
}
