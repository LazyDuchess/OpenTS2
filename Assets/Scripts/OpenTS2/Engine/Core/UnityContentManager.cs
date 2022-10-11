using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content;

namespace OpenTS2.Engine.Core
{
    public class UnityContentManager : ContentManager
    {
        public UnityContentManager() : base()
        {
            _fileSystem = new Files.Filesystem(new JSONPathProvider());
            _provider = new ContentProvider(_fileSystem);
        }
    }
}
