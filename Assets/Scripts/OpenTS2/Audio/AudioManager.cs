using OpenTS2.Common;
using OpenTS2.Common.Utils;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [GameProperty(true)]
        public static uint FXVolume = 100;
        [GameProperty(true)]
        public static uint VOXVolume = 100;
        [GameProperty(true)]
        public static uint AmbienceVolume = 100;
        [GameProperty(true)]
        public static uint MusicVolume = 40;
        [GameProperty(true)]
        public static bool MuteAudioOnFocusLoss = true;

        public static Dictionary<ResourceKey, string> CustomSongNames;
        public static List<ResourceKey> AudioAssets { get; private set; }
        public static Action OnInitialized;
        private static Dictionary<Tuple<uint,uint>, ResourceKey> AudioAssetByInstanceID;
        private static ContentManager ContentManager;

        private void Awake()
        {
            ContentManager = ContentManager.Instance;
            Core.OnFinishedLoading += Initialize;
        }

        private void LoadCustomMusic()
        {
            CustomSongNames = new Dictionary<ResourceKey, string>();
            var musicDir = Path.Combine(Filesystem.GetUserPath(), "Music");
            var stationDirs = Directory.GetDirectories(musicDir);
            foreach(var stationDir in stationDirs)
            {
                var stationName = Path.GetFileName(stationDir);
                var audioFiles = Directory.GetFiles(stationDir, "*.*").Where(file => file.ToLowerInvariant().EndsWith(".mp3") || file.ToLowerInvariant().EndsWith(".wav"));
                foreach(var audioFile in audioFiles)
                {
                    var songName = Path.GetFileNameWithoutExtension(audioFile);
                    var key = new ResourceKey(songName, FileUtils.LowHash(stationName), TypeIDs.AUDIO);
                    ContentManager.MapFileToResource(audioFile, key);
                    CustomSongNames[key] = songName;
                }
            }
        }

        private void Initialize()
        {
            LoadCustomMusic();
            AudioAssetByInstanceID = new Dictionary<Tuple<uint, uint>, ResourceKey>();
            AudioAssets = ContentManager.ResourceMap.Keys.Where(key => key.TypeID == TypeIDs.AUDIO || key.TypeID == TypeIDs.HITLIST).ToList();
            foreach(var audioAsset in AudioAssets)
            {
                AudioAssetByInstanceID[new Tuple<uint, uint>(audioAsset.InstanceID, 0)] = audioAsset;
                AudioAssetByInstanceID[new Tuple<uint,uint>(audioAsset.InstanceID, audioAsset.InstanceHigh)] = audioAsset;
            }
            OnInitialized?.Invoke();
        }

        public static float GetVolumeForMixer(Mixers mixer)
        {
            if (MuteAudioOnFocusLoss && !Application.isFocused)
                return 0f;
            uint val = 100;
            switch (mixer)
            {
                case Mixers.Voices:
                    val = VOXVolume;
                    break;

                case Mixers.SoundEffects:
                    val = FXVolume;
                    break;

                case Mixers.Music:
                    val = MusicVolume;
                    break;

                case Mixers.Ambient:
                    val = AmbienceVolume;
                    break;
            }
            return val / 100f;
        }

        public static ResourceKey GetAudioResourceKeyByInstanceID(uint instanceID, uint instanceIDHigh = 0)
        {
            if (AudioAssetByInstanceID.TryGetValue(new Tuple<uint,uint>(instanceID, instanceIDHigh), out var result))
            {
                return result;
            }
            return default;
        }

        public static void PlayUISound(ResourceKey audioKey)
        {
            var asset = ContentManager.GetAsset<AudioAsset>(audioKey);
            if (asset != null)
            {
                var audioSource = CreateOneShotAudioSource();
                audioSource.Audio = asset;
                audioSource.Mixer = Mixers.SoundEffects;
                audioSource.Play();
            }
        }

        private static TSAudioSource CreateOneShotAudioSource()
        {
            var go = new GameObject("One Shot Audio Source");
            var audioSource = go.AddComponent<TSAudioSource>();
            audioSource.Loop = false;
            audioSource.PlaybackFinished += () =>
            {
                Destroy(go);
            };
            return audioSource;
        }
    }
}
