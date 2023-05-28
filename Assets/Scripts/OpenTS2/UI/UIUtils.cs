using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    public static class UIUtils
    {
        public static Texture2D[] SplitTextureHorizontalSequence(Texture2D texture)
        {
            return SplitTextureHorizontalSequence(texture, texture.height);
        }
        public static Texture2D[] SplitTextureHorizontalSequence(Texture2D texture, int size)
        {
            var amount = texture.width / size;
            var array = new Texture2D[amount];
            for(var i=0;i<amount;i++)
            {
                var currentX = i * size;
                var pixels = texture.GetPixels(currentX, 0, size, texture.height);
                var newTexture = new Texture2D(size, texture.height);
                newTexture.SetPixels(0,0,size,texture.height, pixels);
                newTexture.Apply();
                array[i] = newTexture;
            }
            return array;
        }
    }
}
