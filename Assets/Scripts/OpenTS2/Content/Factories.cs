using OpenTS2.Content.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class Factories
    {
        public static Factories Get
        {
            get
            {
                return _singleton;
            }
        }
        private static Factories _singleton;
        protected ITextureFactory _textureFactory;
        public ITextureFactory TextureFactory
        {
            get
            {
                return _textureFactory;
            }
        }
        public Factories()
        {
            _singleton = this;
        }
    }
}
