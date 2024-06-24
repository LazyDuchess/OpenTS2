using OpenTS2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    public class HitListAsset : AudioAsset
    {
        public ResourceKey[] Sounds;

        public HitListAsset(ResourceKey[] sounds)
        {
            Sounds = sounds;
        }

        public AudioAsset GetRandomAudioAsset()
        {
            return ContentProvider.Get().GetAsset<AudioAsset>(Sounds[UnityEngine.Random.Range(0, Sounds.Length)]);
        }
    }
}
