using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class MP3AudioAsset : AudioAsset
    {
        public byte[] AudioData;

        public MP3AudioAsset(byte[] data)
        {
            AudioData = data;
        }
    }
}
