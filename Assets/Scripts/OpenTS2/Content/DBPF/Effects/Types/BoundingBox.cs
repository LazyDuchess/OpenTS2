using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Content.DBPF.Effects.Types
{
    public readonly struct BoundingBox
    {
        public readonly Vector3 LowerCorner;
        public readonly Vector3 UpperCorner;

        private BoundingBox(Vector3 lowerCorner, Vector3 upperCorner) =>
            (LowerCorner, UpperCorner) = (lowerCorner, upperCorner);

        public static BoundingBox Deserialize(IoBuffer reader)
        {
            return new BoundingBox(
                Vector3Serializer.Deserialize(reader), Vector3Serializer.Deserialize(reader));
        }
    }
}