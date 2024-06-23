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
            _audioSource = gameObject.AddComponent<AudioSource>();
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
        _timeAudioSourcePlaying = 0f;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = Volume;
        _audioSource.clip = (_audio as WAVAudioAsset).Clip;
        _audioSource.loop = Loop;
        _audioSource.Play();
    }

    private void Update()
    {
        if (_audioSource.clip != null)
        {
            _timeAudioSourcePlaying += Time.unscaledDeltaTime;
            if (_timeAudioSourcePlaying > _audioSource.clip.length)
            {
                PlaybackFinished?.Invoke();
                _timeAudioSourcePlaying = 0f;
            }
        }
        else
            _timeAudioSourcePlaying = 0f;

        _audioSource.volume = Volume;
        _audioSource.loop = Loop;
        if (_waveOutEv != null)
        {
            _waveOutEv.Volume = Volume;
        }
    }

    private void WaveOutPlaybackStopped(object sender, StoppedEventArgs e)
    {
        PlaybackFinished?.Invoke();
        if (Loop)
            Play();
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
