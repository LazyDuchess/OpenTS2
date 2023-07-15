using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Types
{
    public class Vector2Serializer
    {
        public static Vector2 Deserialize(IoBuffer reader)
        {
            return new Vector2(reader.ReadFloat(), reader.ReadFloat());
        }
    }
}