using OpenTS2.Content.DBPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicController : MonoBehaviour
    {
        public static MusicController Instance { get; private set; }
        private AudioSource _audioSource;
        private AudioAsset _currentAudioAsset;

        private void Awake()
        {
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
        }

        public static void PlayMusic(AudioAsset music)
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
