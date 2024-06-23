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
        public static MusicController Instance { get; private set; }
        private TSAudioSource _tsAudioSource;
        private MusicCategory _currentMusicCategory = null;

        public void StartMusicCategory(string musicCategory)
        {
            _tsAudioSource.PlaybackFinished -= OnSongEnd;
            _tsAudioSource.PlaybackFinished += OnSongEnd;
            _tsAudioSource.Loop = false;
            _tsAudioSource.Volume = 0.5f;
            _currentMusicCategory = MusicManager.Instance.GetMusicCategory(musicCategory);
            PlayNextSong();
        }

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
            StartMusicCategory("NHood");
        }

        private void PlayNextSong()
        {
            var contentProvider = ContentProvider.Get();
            var currentSong = _currentMusicCategory.PopNextSong();
            var songAsset = contentProvider.GetAsset<AudioAsset>(currentSong.Key);
            _tsAudioSource.Audio = songAsset;
            _tsAudioSource.Play();
        }

        private void OnSongEnd()
        {
            PlayNextSong();
        }
    }
}
