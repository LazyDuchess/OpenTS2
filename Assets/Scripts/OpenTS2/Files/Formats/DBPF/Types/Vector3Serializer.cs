using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Types
{
    public class Vector3Serializer
    {
        public static Vector3 Deserialize(IoBuffer reader)
        {
            return new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        }
    }
}