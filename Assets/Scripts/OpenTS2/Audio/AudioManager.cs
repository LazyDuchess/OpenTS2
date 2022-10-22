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
        readonly static ResourceKey splashKey = new ResourceKey(0xFF8DFFC2, 0xE4085DD2, 0xFF8DFFC2, 0x2026960B);
        public static AudioAsset SplashAudio
        {
            get
            {
                var content = ContentManager.Get();
                return content.Provider.GetAsset<AudioAsset>(splashKey);
            }
        }
    }
}
