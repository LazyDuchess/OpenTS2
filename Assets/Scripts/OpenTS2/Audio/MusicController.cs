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
using Unity.Collections;
using System.Security.Cryptography;

namespace OpenTS2.Audio
{
    [RequireComponent(typeof(TSAudioSource))]
    public class MusicController : MonoBehaviour
    {
        public static MusicController Instance { get; private set; }
        private const float FadeSpeed = 0.5f;
        private TSAudioSource _tsAudioSource;
        private MusicCategory _currentMusicCategory = null;
        private enum States
        {
            Stopped,
            Playing,
            Paused,
            FadingOut,
            FadingIn
        }
        private States _state = States.Stopped;
        private float _currentVolumeMultiplier = 1f;

        public void Stop()
        {
            if (_state == States.Stopped) return;
            _currentMusicCategory = null;
            if (_state == States.Paused)
            {
                _tsAudioSource.Stop();
                _state = States.Stopped;
                return;
            }
            _state = States.FadingOut;
        }

        public void Pause()
        {
            _state = States.Paused;
            _tsAudioSource.Pause();
        }

        public void Resume()
        {
            _state = States.Playing;
            _tsAudioSource.Resume();
        }

        private void UpdateVolumeMultiplier()
        {
            if (_state == States.FadingOut)
            {
                _currentVolumeMultiplier -= FadeSpeed * Time.unscaledDeltaTime;
                if (_currentVolumeMultiplier < 0f)
                {
                    _currentVolumeMultiplier = 0f;
                    if (_currentMusicCategory == null)
                    {
                        _tsAudioSource.Stop();
                        _state = States.Stopped;
                    }
                    else
                    {
                        PlayNextSong();
                        _state = States.FadingIn;
                    }
                }
            }
            else if (_state == States.FadingIn)
            {
                _currentVolumeMultiplier += FadeSpeed * Time.unscaledDeltaTime;
                if (_currentVolumeMultiplier >= 1f)
                {
                    _currentVolumeMultiplier = 1f;
                    _state = States.Playing;
                }
            }
            else
                _currentVolumeMultiplier = 1f;
            if (_state == States.Stopped)
                _currentVolumeMultiplier = 0f;
        }

        private void UpdateVolume()
        {
            _tsAudioSource.Volume = _currentVolumeMultiplier;
        }

        public void StartMusicCategory(string musicCategory)
        {
            _tsAudioSource.PlaybackFinished -= OnSongEnd;
            _tsAudioSource.PlaybackFinished += OnSongEnd;
            _tsAudioSource.Loop = false;
            _currentMusicCategory = MusicManager.Instance.GetMusicCategory(musicCategory);
            _state = States.FadingOut;
            if (_state == States.Stopped)
            {
                _state = States.FadingIn;
                PlayNextSong();
            }
        }

        private void Awake()
        {
            Instance = this;
            _tsAudioSource = GetComponent<TSAudioSource>();
            UpdateVolume();
            Core.OnNeighborhoodEntered += OnNeighborhoodEntered;
            Core.OnBeginLoading += OnBeginLoadingScreen;
        }

        private void Update()
        {
            UpdateVolumeMultiplier();
            UpdateVolume();
        }

        private void OnBeginLoadingScreen()
        {
            _tsAudioSource.Audio = MusicManager.SplashAudio;
            _tsAudioSource.Loop = true;
            _tsAudioSource.Play();
            _state = States.Playing;
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
            if (_state == States.FadingOut)
                return;
            PlayNextSong();
        }
    }
}
