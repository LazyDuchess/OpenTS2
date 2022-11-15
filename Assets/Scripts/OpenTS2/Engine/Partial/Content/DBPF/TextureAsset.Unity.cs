using UnityEngine;

namespace OpenTS2.Content.DBPF
{
    public partial class TextureAsset
    {
        public Texture2D Texture
        {
            get
            {
                return _engineTexture as Texture2D;
            }
            set
            {
                _engineTexture = value;
            }
        }
    }
}
