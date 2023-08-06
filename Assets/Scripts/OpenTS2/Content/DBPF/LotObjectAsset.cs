using UnityEngine;

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// An object placed in a lot.
    ///
    /// https://simswiki.info/wiki.php?title=OBJT
    /// </summary>
    public class LotObjectAsset : AbstractAsset
    {
        public string ResourceName { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public LotObjectAsset(string resourceName, Vector3 position, Quaternion rotation)
        {
            ResourceName = resourceName;
            Position = position;
            Rotation = rotation;
        }
    }
}