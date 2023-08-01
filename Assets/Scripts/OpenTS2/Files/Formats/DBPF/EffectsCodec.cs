using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Content.DBPF.Effects;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.EFFECTS)]
    public class EffectsCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var version = reader.ReadUInt16();
            Debug.Assert(version == 0);

            var particleEffects = new ParticleEffect[reader.ReadUInt32()];

            return new EffectsAsset(particleEffects);
        }
    }
}