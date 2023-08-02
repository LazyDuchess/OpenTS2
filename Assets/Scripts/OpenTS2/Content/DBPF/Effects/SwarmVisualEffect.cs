using OpenTS2.Content.DBPF.Effects.Types;
using OpenTS2.Files.Utils;

namespace OpenTS2.Content.DBPF.Effects
{
    public readonly struct SwarmVisualEffect : IBaseEffect
    {
        public readonly EffectDescription[] Descriptions;

        public SwarmVisualEffect(EffectDescription[] descriptions)
        {
            Descriptions = descriptions;
        }
    }

    public struct EffectDescription
    {
        public string EffectName { get; }
        public byte EffectType { get; }
        public int EffectIndex { get; }

        private EffectDescription(string effectName, byte effectType, int effectIndex)
        {
            EffectName = effectName;
            EffectType = effectType;
            EffectIndex = effectIndex;
        }

        public static EffectDescription Deserialize(IoBuffer reader, ushort version)
        {
            var baseEffect = reader.ReadUint32PrefixedString();
            var blockType = reader.ReadByte();
            var flags = reader.ReadUInt32();
            var transform = EffectsTransform.Deserialize(reader);

            var lodBegin = reader.ReadByte();
            var lodEnd = reader.ReadByte();

            var emitScaleBegin = reader.ReadFloat();
            var emitScaleEnd = reader.ReadFloat();
            var sizeScaleBegin = reader.ReadFloat();
            var sizeScaleEnd = reader.ReadFloat();
            var alphaScaleBegin = reader.ReadFloat();
            var alphaScaleEnd = reader.ReadFloat();

            if (version < 3)
            {
                reader.ReadUInt16();
                reader.ReadUInt16();
            }

            var selectionGroup = reader.ReadUInt16();
            var selectionChance = reader.ReadUInt16();
            var timeScale = reader.ReadFloat();
            var blockIndex = reader.ReadInt32();
            return new EffectDescription(baseEffect, blockType, blockIndex);
        }
    }
}