using UnityEngine;

namespace OpenTS2.Content.DBPF
{
    public partial class TextureAsset
    {
        public Texture2D Texture
        {
            get
            {
                return engineTexture as Texture2D;
            }
            set
            {
                engineTexture = value;
            }
        }
    }
}
