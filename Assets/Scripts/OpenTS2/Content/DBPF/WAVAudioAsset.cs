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
        public AudioClip Clip { get; private set; }

        public WAVAudioAsset(byte[] data)
        {
            Clip = WAV.ToAudioClip(data, GlobalTGI.ToString());
        }

        public override void FreeUnmanagedResources()
        {
            if (Clip == null)
                return;
            Clip.Free();
        }
    }
}
