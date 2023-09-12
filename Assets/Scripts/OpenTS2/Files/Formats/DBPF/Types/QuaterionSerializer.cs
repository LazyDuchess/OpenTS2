using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Types
{
    public static class QuaternionSerializer
    {
        public static Quaternion Deserialize(IoBuffer reader)
        {
            return new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        }

        public static Quaternion FromSims2EulerRotationDegrees(float x, float y, float z)
        {
            var sx = Mathf.Sin(x * 0.5f * Mathf.Deg2Rad);
            var cx = Mathf.Cos(x * 0.5f * Mathf.Deg2Rad);
            var sy = Mathf.Sin(y * 0.5f * Mathf.Deg2Rad);
            var cy = Mathf.Cos(y * 0.5f * Mathf.Deg2Rad);
            var sz = Mathf.Sin(z * 0.5f * Mathf.Deg2Rad);
            var cz = Mathf.Cos(z * 0.5f * Mathf.Deg2Rad);

            var quatX = (cz * (cy * sx)) - (sz * (cx * sy));
            var quatY = (cz * (cx * sy)) + (sz * (cy * sx));
            var quatZ = (sz * (cx * cy)) - (cz * (sx * sy));
            var quatW = (cz * (cx * cy)) + (sz * (sx * sy));
            return new Quaternion(quatX, quatY, quatZ, quatW);
        }
    }
}