using NAudio.Wave;
using OpenTS2.Content.DBPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public bool Loop = false;
    public float Volume = 1f;
    public Action PlaybackFinished;
    private AudioAsset _audio;
    private WaveOutEvent _waveOutEv;
    private AudioSource _audioSource;
    private float _timeAudioSourcePlaying = 0f;
    private bool _audioClipPlaying = false;

    public void Play()
    {
        CleanUp();
        if (Audio is MP3AudioAsset)
            PlayMP3();
        else
            PlayWAV();
    }

    public void Stop()
    {
        CleanUp();
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    private void CleanUp()
    {
        if (_waveOutEv != null)
        {
            _waveOutEv.PlaybackStopped -= WaveOutPlaybackStopped;
            _waveOutEv.Dispose();
        }
        _audioSource.Stop();
        _audioSource.clip = null;
        _audioClipPlaying = false;
        _timeAudioSourcePlaying = 0f;
    }

    private void PlayMP3()
    {
        var stream = new MemoryStream((_audio as MP3AudioAsset).AudioData);
        var reader = new Mp3FileReader(stream);
        _waveOutEv = new WaveOutEvent();
        _waveOutEv.Init(reader);
        _waveOutEv.Volume = Volume;
        _waveOutEv.Play();
        _waveOutEv.PlaybackStopped += WaveOutPlaybackStopped;
    }

    private void PlayWAV()
    {
        _audioClipPlaying = true;
        _timeAudioSourcePlaying = 0f;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = Volume;
        _audioSource.clip = (_audio as WAVAudioAsset).Clip;
        _audioSource.loop = Loop;
        _audioSource.Play();
    }

    private void Update()
    {
        _audioSource.loop = Loop;
        _audioSource.volume = Volume;

        if (_audioSource.clip != null && _audioClipPlaying && !Loop)
        {
            _timeAudioSourcePlaying += Time.unscaledDeltaTime;
            if (_timeAudioSourcePlaying >= _audioSource.clip.length && !_audioSource.isPlaying)
            {
                _audioClipPlaying = false;
                _timeAudioSourcePlaying = 0f;
                PlaybackFinished?.Invoke();
            }
        }
        else
            _timeAudioSourcePlaying = 0f;
        
        if (_waveOutEv != null)
        {
            _waveOutEv.Volume = Volume;
        }
    }

    private void WaveOutPlaybackStopped(object sender, StoppedEventArgs e)
    {
        if (Loop)
            Play();
        else
            PlaybackFinished?.Invoke();
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
