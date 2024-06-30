using OpenTS2.Audio;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Wrapper for Unity audio source and NAudio, for playing WAV and MP3.
/// </summary>
public class TSAudioSource : MonoBehaviour
{
    public AudioAsset Audio
    {
        get
        {
            return _audio;
        }
        set
        {
            _audio = value;
            CleanUp();
        }
    }
    public Mixers Mixer = Mixers.SoundEffects;
    public bool Loop = false;
    public float Volume = 1f;
    public Action PlaybackFinished;
    private AudioAsset _audio;
    private AudioSource _audioSource;
    private float _timeAudioSourcePlaying = 0f;
    private bool _audioClipPlaying = false;
    private bool _paused = false;
    private ContentManager _contentManager;

    public void Pause()
    {
        if (_paused)
            return;
        _audioSource.Pause();
        _paused = true;
    }

    public void Resume()
    {
        if (!_paused)
            return;
        _audioSource.UnPause();
        _paused = false;
    }

    public void Play()
    {
        CleanUp();
        PlayInternal(Audio);
    }

    public void PlayAsync(ResourceKey audioResourceKey)
    {
        CleanUp();
        Task.Run(() =>
        {
            _audio = _contentManager.GetAsset<AudioAsset>(audioResourceKey);
        }).ContinueWith(task =>
        {
            PlayInternal(_audio);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void PlayInternal(AudioAsset asset)
    {
        if (asset is HitListAsset)
        {
            asset = (asset as HitListAsset).GetRandomAudioAsset();
            PlayInternal(asset);
            return;
        }
        PlayWAV(asset);
    }

    public void Stop()
    {
        CleanUp();
    }

    private void Awake()
    {
        _contentManager = ContentManager.Instance;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    private void CleanUp()
    {
        StopAllCoroutines();
        _audioSource.Stop();
        _audioSource.clip = null;
        _audioClipPlaying = false;
        _timeAudioSourcePlaying = 0f;
        _paused = false;
    }

    private void PlayWAV(AudioAsset asset)
    {
        _audioClipPlaying = true;
        _timeAudioSourcePlaying = 0f;
        _audioSource.spatialBlend = 0f;
        UpdateVolume();
        _audioSource.clip = (asset as WAVAudioAsset).Clip;
        _audioSource.loop = Loop;
        _audioSource.Play();
    }

    private void UpdateVolume()
    {
        var volume = Volume * AudioManager.Instance.GetVolumeForMixer(Mixer);
        _audioSource.volume = volume;
    }

    private void Update()
    {
        _audioSource.loop = Loop;
        UpdateVolume();

        if (_audioSource.clip != null && _audioClipPlaying && !Loop)
        {
            if (!_paused)
            {
                _timeAudioSourcePlaying += Time.unscaledDeltaTime;
                if (_timeAudioSourcePlaying >= _audioSource.clip.length && !_audioSource.isPlaying)
                {
                    _audioClipPlaying = false;
                    _timeAudioSourcePlaying = 0f;
                    PlaybackFinished?.Invoke();
                }
            }
        }
        else
            _timeAudioSourcePlaying = 0f;
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
