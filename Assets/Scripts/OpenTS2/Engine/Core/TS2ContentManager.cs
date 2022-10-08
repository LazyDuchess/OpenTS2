using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content;

namespace OpenTS2.Engine.Core
{
    public class TS2ContentManager : ContentManager
    {
        public static new TS2ContentManager Get
        {
            get
            {
                return _singleton;
            }
        }
        static new TS2ContentManager _singleton;
        public TS2ContentManager() : base()
        {
            _singleton = this;
            _textureFactory = new TextureFactory();
            _fileSystem = new Files.Filesystem(new JSONPathProvider());
            _provider = new ContentProvider(_fileSystem);
        }
    }
}
