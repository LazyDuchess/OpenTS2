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
                if (_clip == null)
                    _clip = WavUtility.ToAudioClip(AudioData, 0, TGI.ToString());
                return _clip;
            }
        }
        AudioClip _clip;
    }
}
