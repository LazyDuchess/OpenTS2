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
    [RequireComponent(typeof(TSAudioSource))]
    public class MusicController : MonoBehaviour
    {
        private WaveOutEvent _waveOut = null;
        public static MusicController Instance { get; private set; }
        private TSAudioSource _tsAudioSource;
        private AudioAsset _currentAudioAsset;
        private MusicCategory _currentMusicCategory = null;

        private void Awake()
        {
            Instance = this;
            _tsAudioSource = GetComponent<TSAudioSource>();
            Core.OnNeighborhoodEntered += OnNeighborhoodEntered;
            Core.OnStartup += OnStartup;
        }

        private void OnStartup()
        {
            _tsAudioSource.Audio = MusicManager.SplashAudio;
            _tsAudioSource.Volume = 0.5f;
            _tsAudioSource.Loop = true;
            _tsAudioSource.Play();
        }

        private void OnNeighborhoodEntered()
        {
            StopAllCoroutines();
            _tsAudioSource.PlaybackFinished -= OnSongEnd;
            _tsAudioSource.PlaybackFinished += OnSongEnd;
            _tsAudioSource.Loop = false;
            _tsAudioSource.Volume = 0.5f;
            _currentMusicCategory = MusicManager.Instance.GetMusicCategory("NHood");
            PlayNextSong();
        }

        private void PlayNextSong()
        {
            var contentProvider = ContentProvider.Get();
            var currentSong = _currentMusicCategory.PopNextSong();
            var songAsset = contentProvider.GetAsset<MP3AudioAsset>(currentSong.Key);
            _tsAudioSource.Audio = songAsset;
            _tsAudioSource.Play();
        }

        private void OnSongEnd()
        {
            PlayNextSong();
        }

        

        public static void FadeOutMusic()
        {
            Instance.StartCoroutine(Instance.FadeOut());
        }

        IEnumerator FadeOut()
        {
            var volume = _tsAudioSource.Volume;
            while (volume > 0f)
            {
                volume -= 0.5f * Time.deltaTime;
                _tsAudioSource.Volume = volume;
                yield return null;
            }
            StopMusic();
        }

        void StopMusic()
        {
            _tsAudioSource.Stop();
        }
    }
}
