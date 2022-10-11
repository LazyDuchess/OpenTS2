using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Content;

namespace OpenTS2.Engine.Core
{
    public class UnityFactories : Factories
    {
        public UnityFactories() : base()
        {
            _textureFactory = new TextureFactory();
        }
    }
}
