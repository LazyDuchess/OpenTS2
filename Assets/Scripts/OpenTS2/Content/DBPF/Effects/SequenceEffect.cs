using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Effects
{
    /// <summary>
    /// Plays a bunch of effects in a sequence.
    /// </summary>
    public readonly struct SequenceEffect : IBaseEffect
    {
        public SequenceComponent[] Components { get; }
        public uint Flags { get; }

        public SequenceEffect(SequenceComponent[] components, uint flags)
        {
            Components = components;
            Flags = flags;
        }
    }

    public readonly struct SequenceComponent
    {
        public readonly Vector2 ActivateTime;
        public readonly string EffectName;

        public SequenceComponent(Vector2 activateTime, string effectName)
        {
            ActivateTime = activateTime;
            EffectName = effectName;
        }

        public static SequenceComponent Deserialize(IoBuffer reader)
        {
            return new SequenceComponent(Vector2Serializer.Deserialize(reader), reader.ReadUint32PrefixedString());
        }
    }
}