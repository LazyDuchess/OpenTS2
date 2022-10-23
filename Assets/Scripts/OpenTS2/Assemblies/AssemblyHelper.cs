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
        /// <summary>
        /// Initializes an assembly, does Reflection tasks like parsing attributes.
        /// </summary>
        /// <param name="assembly"></param>
        public static void InitializeAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<CodecAttribute>();
                if (attr != null)
                {
                    var instance = Activator.CreateInstance(type) as AbstractCodec;
                    foreach (var element in attr.TypeIDs)
                    {
                        Codecs.Register(element, instance);
                    }
                }
            }
        }
    }
}
