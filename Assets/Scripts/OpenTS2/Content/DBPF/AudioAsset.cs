using OpenTS2.Engine;
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
        public AudioClip Clip => _clip;
        AudioClip _clip;
        public AudioAsset(byte[] data)
        {
            AudioData = data;
            _clip = WAV.ToAudioClip(AudioData, GlobalTGI.ToString());
        }
        public override void FreeUnmanagedResources()
        {
            if (_clip == null)
                return;
            _clip.Free();
        }
    }
}
