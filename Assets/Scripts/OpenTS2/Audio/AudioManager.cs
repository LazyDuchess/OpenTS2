using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static List<ResourceKey> AudioAssets { get; private set; }
        public static Action OnInitialized;
        private static Dictionary<uint, ResourceKey> AudioAssetByLowInstanceID;
        private static Dictionary<ResourceKey, ResourceKey> AudioAssetByInstanceID;
        private static ContentProvider ContentProvider;

        private void Awake()
        {
            ContentProvider = ContentProvider.Get();
            Core.OnFinishedLoading += Initialize;
        }

        private void Initialize()
        {
            AudioAssetByLowInstanceID = new Dictionary<uint, ResourceKey>();
            AudioAssetByInstanceID = new Dictionary<ResourceKey, ResourceKey>();
            AudioAssets = ContentProvider.ResourceMap.Keys.Where(key => key.TypeID == TypeIDs.AUDIO || key.TypeID == TypeIDs.HITLIST).ToList();
            foreach(var audioAsset in AudioAssets)
            {
                AudioAssetByLowInstanceID[audioAsset.InstanceID] = audioAsset;
                AudioAssetByInstanceID[new ResourceKey(audioAsset.InstanceID, audioAsset.InstanceHigh, 0, 0)] = audioAsset;
            }
            OnInitialized?.Invoke();
        }

        // TODO
        public static float GetVolumeForMixer(Mixers mixer)
        {
            return 0.5f;
        }

        public static ResourceKey GetAudioResourceKeyByInstanceID(uint instanceID)
        {
            if (AudioAssetByLowInstanceID.TryGetValue(instanceID, out var result))
            {
                return result;
            }
            return default;
        }

        public static ResourceKey GetAudioResourceKeyByInstanceID(uint instanceID, uint instanceIDHigh)
        {
            var key = new ResourceKey(instanceID, instanceIDHigh, 0, 0);
            if (AudioAssetByInstanceID.TryGetValue(key, out var result))
            {
                return result;
            }
            return default;
        }

        public static void PlayUISound(ResourceKey audioKey)
        {
            var asset = ContentProvider.GetAsset<AudioAsset>(audioKey);
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
