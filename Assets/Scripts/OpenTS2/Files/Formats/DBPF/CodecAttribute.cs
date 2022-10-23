using OpenTS2.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Register this class as a Codec for serializing and deserializing resources of the specified Type ID(s).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CodecAttribute : Attribute
    {
        public readonly uint[] TypeIDs;
        public CodecAttribute(params uint[] TypeIDs)
        {
            this.TypeIDs = TypeIDs;
        }

        public static void Initialize()
        {
            AssemblyHelper.AssemblyProcesses += AssemblyProcess;
        }

        static void AssemblyProcess(Type type, Assembly assembly)
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
