using OpenTS2.Engine.Core;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Files;

namespace OpenTS2.Engine.Core
{
    public static class ResourceManagement
    {
        public static void Initialize()
        {
            Filesystem.SetPathProvider(new JSONPathProvider());
            ContentManager.Provider = new ContentProvider();
            Factories.TextureFactory = new TextureFactory();
        }
    }
}
