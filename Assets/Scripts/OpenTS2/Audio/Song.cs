using OpenTS2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Audio
{
    public class Song
    {
        public ResourceKey Key;
        public string LocalizedName;

        public Song(ResourceKey key, string localizedName)
        {
            Key = key;
            LocalizedName = localizedName;
        }
    }
}
