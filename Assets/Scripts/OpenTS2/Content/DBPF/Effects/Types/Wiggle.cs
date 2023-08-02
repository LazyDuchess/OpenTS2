using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Effects.Types
{
    public readonly struct Wiggle
    {
        public readonly float TimeRate;
        public readonly Vector3 RateDirection;
        public readonly Vector3 WiggleDirection;

        public Wiggle(float timeRate, Vector3 rateDirection, Vector3 wiggleDirection)
        {
            TimeRate = timeRate;
            RateDirection = rateDirection;
            WiggleDirection = wiggleDirection;
        }

        public static Wiggle Deserialize(IoBuffer reader)
        {
            return new Wiggle(reader.ReadFloat(), Vector3Serializer.Deserialize(reader),
                Vector3Serializer.Deserialize(reader));
        }

        public static Wiggle[] DeserializeList(IoBuffer reader)
        {
            var wiggles = new Wiggle[reader.ReadUInt32()];
            for (var i = 0; i < wiggles.Length; i++)
            {
                wiggles[i] = Deserialize(reader);
            }

            return wiggles;
        }
    }
}