using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Types
{
    public class QuaterionSerialzier
    {
        public static Quaternion Deserialize(IoBuffer reader)
        {
            return new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        }
    }
}