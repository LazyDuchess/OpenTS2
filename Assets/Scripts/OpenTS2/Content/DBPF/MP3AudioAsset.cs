using OpenTS2.Engine;
using OpenTS2.Engine.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NLayer;
using System.IO;

namespace OpenTS2.Content.DBPF
{
    public class MP3AudioAsset : WAVAudioAsset
    {
        public override AudioClip Clip
        {
            get
            {
                if (_clip == null)
                {
                    _clip = AudioClip.Create(GlobalTGI.ToString(),
                        (int)(_mpegFile.Length / sizeof(float) / _mpegFile.Channels),
                        _mpegFile.Channels,
                        _mpegFile.SampleRate,
                        true,
                        OnMp3Read,
                        OnClipPositionSet);
                }
                return _clip;
            }
        }
        public byte[] AudioData;
        private AudioClip _clip;
        private MpegFile _mpegFile;
        private MemoryStream _stream;

        public MP3AudioAsset(byte[] data) : base(data)
        {
            _stream = new MemoryStream(data);
            _mpegFile = new MpegFile(_stream);
        }

        public override void FreeUnmanagedResources()
        {
            if (_clip == null)
                return;
            _clip.Free();
        }

        // PCMReaderCallback will called each time AudioClip reads data.
        private void OnMp3Read(float[] data)
        {
            int actualReadCount = _mpegFile.ReadSamples(data, 0, data.Length);
        }

        // PCMSetPositionCallback will called when first loading this audioclip
        private void OnClipPositionSet(int position)
        {
            _mpegFile = new MpegFile(_stream);
        }
    }
}
