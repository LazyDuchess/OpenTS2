using UnityEngine;

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// This asset corresponds to NHoodOccupantManager, the occupants being the decorations. This is called neighborhood
    /// terrain on the wiki but the actual geometry for the terrain is part of a different file type.
    ///
    /// https://simswiki.info/wiki.php?title=NHTR
    /// </summary>
    public class NeighborhoodDecorationsAsset : AbstractAsset
    {
        public FloraDecoration[] FloraDecorations { get; }

        public NeighborhoodDecorationsAsset(FloraDecoration[] flora) => (FloraDecorations) = flora;
    }

    public struct DecorationPosition
    {
        public Vector3 Position;
        public Vector2 BoundingBoxMin;
        public Vector2 BoundingBoxMax;
        public float Rotation;
    }

    public class FloraDecoration
    {
        public DecorationPosition Position { get; }
        public uint ObjectId { get; }

        public FloraDecoration(DecorationPosition position, uint objectId) =>
            (Position, ObjectId) = (position, objectId);
    }
}