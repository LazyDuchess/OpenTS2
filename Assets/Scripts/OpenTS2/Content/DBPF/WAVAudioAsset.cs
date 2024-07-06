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
        public virtual AudioClip Clip
        {
            get
            {
                if (_clip == null)
                    _clip = WAV.ToAudioClip(Data, GlobalTGI.ToString());
                return _clip;
            }
        }
        protected byte[] Data;
        private AudioClip _clip;

        public WAVAudioAsset(byte[] data)
        {
            Data = data;
        }

        public override void FreeUnmanagedResources()
        {
            if (_clip == null)
                return;
            _clip.Free();
        }

#if UNITY_EDITOR
        public byte[] GetWAVData()
        {
            return Data;
        }
#endif
    }
}
