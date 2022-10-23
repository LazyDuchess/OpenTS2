using OpenTS2.Engine.Core;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Files;
using OpenTS2.Client;
using OpenTS2.Assemblies;
using System.Reflection;
using OpenTS2.Files.Formats.DBPF;

namespace OpenTS2.Engine.Core
{
    public static class ResourceManagement
    {
        public static void Initialize()
        {
            var settings = new Settings();
            var epManager = new EPManager();
            var contentManager = new ContentManager();
            var objectManager = new ObjectManager(contentManager.Provider);
            Filesystem.Initialize(new JSONPathProvider(), epManager);
            Factories.TextureFactory = new TextureFactory();
            CodecAttribute.Initialize();
            AssemblyHelper.InitializeAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
