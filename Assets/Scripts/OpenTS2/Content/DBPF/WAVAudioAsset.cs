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
    public class WAVAudioAsset : AudioAsset
    {
        public AudioClip Clip
        {
            get
            {
                if (_clip == null)
                    _clip = WAV.ToAudioClip(_data, GlobalTGI.ToString());
                return _clip;
            }
        }
        private AudioClip _clip;
        private byte[] _data;

        public WAVAudioAsset(byte[] data)
        {
            _data = data;
        }

        public override void FreeUnmanagedResources()
        {
            if (_clip == null)
                return;
            _clip.Free();
        }
    }
}
