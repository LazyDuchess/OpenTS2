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

        /// <summary>
        /// These bounding boxes usually represent cuboids, ellipsoids or toruses. They are represented by the two
        /// corners of the shape.
        ///
        /// In Unity land shapes are usually represented by their center, scale and rotation from a unit cube, sphere.
        /// This function handles converting corners to the Unity style.
        /// </summary>
        /// <returns>
        /// The center position of the box, its rotation and scale as a triple in that order.
        /// </returns>
        public readonly (Vector3, Quaternion, Vector3) GetCenterRotationAndScale()
        {
            var center = (LowerCorner + UpperCorner) / 2;

            // TODO: figure out how to calculate rotation.
            var rotation = Quaternion.identity;

            // TODO: this might be wrong.
            var scale = (UpperCorner - LowerCorner);

            return (center, rotation, scale);
        }
    }
}