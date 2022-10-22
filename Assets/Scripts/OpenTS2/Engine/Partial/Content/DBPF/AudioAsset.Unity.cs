using UnityEngine;
using OpenTS2.Engine.Audio;

namespace OpenTS2.Content.DBPF
{
    public partial class AudioAsset
    {
        public AudioClip Clip
        {
            get
            {
                if (_Clip == null)
                    _Clip = WavUtility.ToAudioClip(audioData, 0, TGI.ToString());
                return _Clip;
            }
        }
        AudioClip _Clip;
    }
}
