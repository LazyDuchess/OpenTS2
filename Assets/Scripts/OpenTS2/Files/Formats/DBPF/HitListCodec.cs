using OpenTS2.Audio;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// List of audio resources.
    /// </summary>
    [Codec(TypeIDs.HITLIST)]
    public class HitListCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            var version = reader.ReadInt32();
            if (version != 56)
                throw new IOException("Unknown HitList version!");
            var soundCount = reader.ReadInt32();
            var sounds = new ResourceKey[soundCount];
            for (var i = 0; i < soundCount; i++)
            {
                var lowID = reader.ReadUInt32();
                var highID = reader.ReadUInt32();
                var key = AudioManager.GetAudioResourceKeyByInstanceID(lowID, highID);
                sounds[i] = key;
            }
            return new HitListAsset(sounds);
        }
    }
}
