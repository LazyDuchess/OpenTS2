using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Effects.Types
{
    public readonly struct Collider
    {
        public readonly uint Flags;
        public readonly byte UnknownByte;
        public readonly string UnknownString1;
        public readonly Vector3 UnknownVector1;
        public readonly Vector3 UnknownVector2;
        public readonly Vector3 UnknownVector3;
        public readonly float UnknownFloat1;
        public readonly float UnknownFloat2;
        public readonly float UnknownFloat3;
        public readonly float UnknownFloat4;
        public readonly float UnknownFloat5;
        public readonly string UnknownString2;
        public readonly string UnknownString3;

        public Collider(uint flags, byte unknownByte, string unknownString1, Vector3 unknownVector1,
            Vector3 unknownVector2, Vector3 unknownVector3, float unknownFloat1, float unknownFloat2,
            float unknownFloat3, float unknownFloat4, float unknownFloat5, string unknownString2, string unknownString3)
        {
            Flags = flags;
            UnknownByte = unknownByte;
            UnknownString1 = unknownString1;
            UnknownVector1 = unknownVector1;
            UnknownVector2 = unknownVector2;
            UnknownVector3 = unknownVector3;
            UnknownFloat1 = unknownFloat1;
            UnknownFloat2 = unknownFloat2;
            UnknownFloat3 = unknownFloat3;
            UnknownFloat4 = unknownFloat4;
            UnknownFloat5 = unknownFloat5;
            UnknownString2 = unknownString2;
            UnknownString3 = unknownString3;
        }

        public static Collider Deserialize(IoBuffer reader)
        {
            var flags = reader.ReadUInt32();
            byte unknownByte = 0;
            if ((flags & 0x1) == 1)
            {
                unknownByte = reader.ReadByte();
            }
            var unknownString1 = reader.ReadUint32PrefixedString();

            var unknownVector1 = Vector3Serializer.Deserialize(reader);
            var unknownVector2 = Vector3Serializer.Deserialize(reader);
            var unknownVector3 = Vector3Serializer.Deserialize(reader);
            var unknownFloat1 = reader.ReadFloat();
            var unknownFloat2 = reader.ReadFloat();
            var unknownFloat3 = 0.0f;
            if ((flags & 0x1) == 1 && unknownByte != 0)
            {
                unknownFloat3 = reader.ReadFloat();
            }
            var unknownFloat4 = reader.ReadFloat();
            var unknownFloat5 = reader.ReadFloat();

            var unknownString2 = reader.ReadUint32PrefixedString();
            var unknownString3 = reader.ReadUint32PrefixedString();
            return new Collider(flags, unknownByte, unknownString1,
                unknownVector1, unknownVector2, unknownVector3,
                unknownFloat1, unknownFloat2, unknownFloat3, unknownFloat4, unknownFloat5,
                unknownString2, unknownString3);
        }

        public static Collider[] DeserializeList(IoBuffer reader)
        {
            var colliders = new Collider[reader.ReadUInt32()];
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i] = Deserialize(reader);
            }

            return colliders;
        }
    }
}