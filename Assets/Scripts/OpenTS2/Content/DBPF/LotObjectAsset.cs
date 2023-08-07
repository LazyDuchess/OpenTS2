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
        public LotObject Object { get; }

        public LotObjectAsset(LotObject lotObject) => (Object) = (lotObject);


        public class LotObject
        {
            public string ResourceName { get; }
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }

            public LotObject(string resourceName, Vector3 position, Quaternion rotation)
            {
                ResourceName = resourceName;
                Position = position;
                Rotation = rotation;
            }
        }

        public class AnimatableObject : LotObject
        {
            public AnimatableObject(LotObject baseObject) : base(baseObject.ResourceName, baseObject.Position, baseObject.Rotation)
            {
            }
        }
    }
}