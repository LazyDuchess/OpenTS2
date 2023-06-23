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
        public static MusicController Singleton => s_singleton;
        static MusicController s_singleton = null;
        private AudioSource _audioSource;
        private AudioAsset _currentAudioAsset;

        private void Awake()
        {
            if (s_singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            s_singleton = this;
            DontDestroyOnLoad(gameObject);
            _audioSource = GetComponent<AudioSource>();
        }

        public static void PlayMusic(AudioAsset music)
        {
            Singleton._currentAudioAsset = music;
            Singleton._audioSource.clip = music.Clip;
            Singleton._audioSource.volume = 1f;
            Singleton._audioSource.Play();
        }

        public static void FadeOutMusic()
        {
            Singleton.StartCoroutine(Singleton.FadeOut());
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
