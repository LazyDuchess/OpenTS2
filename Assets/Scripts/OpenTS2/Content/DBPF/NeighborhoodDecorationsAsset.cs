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
        public RoadDecoration[] RoadDecorations { get; }
        public RoadWithModelDecoration[] RoadWithModelDecorations { get; }
        public PropDecoration[] PropDecorations { get; }

        public NeighborhoodDecorationsAsset(FloraDecoration[] flora, RoadDecoration[] roads,
            RoadWithModelDecoration[] roadsWithModels, PropDecoration[] props) =>
            (FloraDecorations, RoadDecorations, RoadWithModelDecorations, PropDecorations) =
            (flora, roads, roadsWithModels, props);
    }

    public struct DecorationPosition
    {
        public Vector3 Position;
        public Vector2 BoundingBoxMin;
        public Vector2 BoundingBoxMax;
    }

    public class FloraDecoration
    {
        public DecorationPosition Position { get; }
        public float Rotation;

        public uint ObjectId { get; }

        public FloraDecoration(DecorationPosition position, float rotation, uint objectId) =>
            (Position, Rotation, ObjectId) = (position, rotation, objectId);
    }

    public class RoadDecoration
    {
        public DecorationPosition Position { get; }

        public RoadDecoration(DecorationPosition position) => (Position) = (position);
    }

    /// <summary>
    /// Used for bridges and tunnels mostly.
    /// </summary>
    public class RoadWithModelDecoration
    {
        public RoadDecoration Road;
        public RoadWithModelDecoration(RoadDecoration road) => (Road) = (road);
    }

    public class PropDecoration
    {
        public DecorationPosition Position { get; }

        public float Rotation;

        public uint PropId { get; }

        public PropDecoration(DecorationPosition position, float rotation, uint propId) =>
            (Position, Rotation, PropId) = (position, rotation, propId);
    }
}