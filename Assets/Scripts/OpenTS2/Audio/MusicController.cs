using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NAudio;
using NAudio.Wave;
using OpenTS2.Content;
using System.IO;

namespace OpenTS2.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicController : MonoBehaviour
    {
        private WaveOutEvent _waveOut = null;
        public static MusicController Instance { get; private set; }
        private AudioSource _audioSource;
        private AudioAsset _currentAudioAsset;
        private MusicCategory _currentMusicCategory = null;

        private void Awake()
        {
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
            Core.OnNeighborhoodEntered += OnNeighborhoodEntered;
        }

        private void OnNeighborhoodEntered()
        {
            _currentMusicCategory = MusicManager.Instance.GetMusicCategory("NHood");
            PlayNextSong();
        }

        private void PlayNextSong()
        {
            var contentProvider = ContentProvider.Get();
            var currentSong = _currentMusicCategory.PopNextSong();
            var songAsset = contentProvider.GetAsset<MP3AudioAsset>(currentSong.Key);
            var stream = new MemoryStream(songAsset.AudioData);
            var reader = new Mp3FileReader(stream);
            if (_waveOut != null)
            {
                _waveOut.PlaybackStopped -= OnSongEnd;
                _waveOut.Dispose();
            }
            _waveOut = new WaveOutEvent(); // or WaveOutEvent()
            _waveOut.Init(reader);
            _waveOut.Volume = 0.5f;
            _waveOut.Play();
            _waveOut.PlaybackStopped += OnSongEnd;
        }

        private void OnSongEnd(object sender, StoppedEventArgs e)
        {
            PlayNextSong();
        }

        public void OnApplicationQuit()
        {
            if (_waveOut != null)
            {
                _waveOut.PlaybackStopped -= OnSongEnd;
                _waveOut.Dispose();
            }
        }

        public static void PlaySplashMusic(WAVAudioAsset music)
        {
            Instance._currentAudioAsset = music;
            Instance._audioSource.clip = music.Clip;
            Instance._audioSource.volume = 1f;
            Instance._audioSource.Play();
        }

        public static void FadeOutMusic()
        {
            Instance.StartCoroutine(Instance.FadeOut());
        }

        IEnumerator FadeOut()
        {
            var volume = _audioSource.volume;
            while (volume > 0f)
            {
                volume -= 0.5f * Time.deltaTime;
                _audioSource.volume = volume;
                yield return null;
            }
            StopMusic();
        }

        void StopMusic()
        {
            _audioSource.Stop();
            _currentAudioAsset = null;
        }
    }
}
