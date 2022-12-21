using OpenTS2.Engine.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Content.DBPF
{
    public class AudioAsset : AbstractAsset
    {
        public byte[] AudioData;
        public AudioClip Clip
        {
            get
            {
                if (_clip == null)
                    _clip = WavUtility.ToAudioClip(AudioData, 0, TGI.ToString());
                return _clip;
            }
        }
        AudioClip _clip;
    }
}
