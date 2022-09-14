using OpenTS2.Unity.Content;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Unity.Core
{
    public static class ResourceManagement
    {
        public static void Initialize()
        {
            ContentManager.TextureFactory = new TextureFactory();
            ContentManager.FileSystem = new Files.Filesystem(new JSONPathProvider());
            ContentManager.Provider = new ContentProvider(ContentManager.FileSystem);
        }
    }
}
