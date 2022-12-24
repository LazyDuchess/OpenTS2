using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Audio
{
    public static class AudioManager
    {
        readonly static ResourceKey s_splashKey = new ResourceKey(0xFF8DFFC2, 0xE4085DD2, 0xFF8DFFC2, 0x2026960B);
        public static AudioAsset SplashAudio
        {
            get
            {
                var contentProvider = ContentProvider.Get();
                return contentProvider.GetAsset<AudioAsset>(s_splashKey);
            }
        }
    }
}
