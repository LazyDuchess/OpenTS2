using OpenTS2.Engine;
using OpenTS2.Engine.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using NAudio;
using NAudio.Wave;
using System.Collections;

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
                        (int)(_mp3Reader.Length / sizeof(float)),
                        _sampleProvider.WaveFormat.Channels,
                        _sampleProvider.WaveFormat.SampleRate,
                        true,
                        OnMp3Read,
                        OnClipPositionSet);
                }
                return _clip;
            }
        }
        public byte[] AudioData;
        private AudioClip _clip;
        private Mp3FileReader _mp3Reader;
        private ISampleProvider _sampleProvider;

        public MP3AudioAsset(byte[] data) : base(data)
        {
            var stream = new MemoryStream(data);
            _mp3Reader = new Mp3FileReader(stream);
            _sampleProvider = _mp3Reader.ToSampleProvider();
        }

        public override void FreeUnmanagedResources()
        {
            if (_clip == null)
                return;
            _clip.Free();
        }

        private void OnMp3Read(float[] data)
        {
            _sampleProvider.Read(data, 0, data.Length);
        }

        private void OnClipPositionSet(int position)
        {
            // Hallo :3
            //_mp3Reader = new Mp3FileReader(_stream);
        }
    }
}
