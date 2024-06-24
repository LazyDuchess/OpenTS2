using OpenTS2.Common;
using OpenTS2.Content;
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
        private static Dictionary<uint, ResourceKey> _audioAssetByLowInstanceID;
        private static ContentProvider _contentProvider;

        private void Awake()
        {
            _contentProvider = ContentProvider.Get();
            Core.OnFinishedLoading += Initialize;
        }

        private void Initialize()
        {
            AudioAssets = _contentProvider.ResourceMap.Keys.Where(key => key.TypeID == TypeIDs.AUDIO || key.TypeID == TypeIDs.HITLIST).ToList();
            foreach(var audioAsset in AudioAssets)
            {
                _audioAssetByLowInstanceID[audioAsset.InstanceID] = audioAsset;
            }
            OnInitialized?.Invoke();
        }
    }
}
