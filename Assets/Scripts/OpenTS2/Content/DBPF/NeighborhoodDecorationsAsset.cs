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
        public BridgeDecoration[] BridgeDecorations { get; }
        public PropDecoration[] PropDecorations { get; }

        public NeighborhoodDecorationsAsset(FloraDecoration[] flora, RoadDecoration[] roads,
            BridgeDecoration[] roadsWithModels, PropDecoration[] props) =>
            (FloraDecorations, RoadDecorations, BridgeDecorations, PropDecorations) =
            (flora, roads, roadsWithModels, props);
    }

    public struct DecorationPosition
    {
        public Vector3 Position;
        public Vector2 BoundingBoxMin;
        public Vector2 BoundingBoxMax;
    }

    /// <summary>
    /// Decoration object that has a guid to map to a model, position and rotation.
    /// </summary>
    public class DecorationWithObjectId
    {
        public DecorationPosition Position { get; }
        public float Rotation { get; }
        public uint ObjectId { get; }

        public DecorationWithObjectId(DecorationPosition position, float rotation, uint objectId) =>
            (Position, Rotation, ObjectId) = (position, rotation, objectId);
    }

    public class FloraDecoration : DecorationWithObjectId
    {
        public FloraDecoration(DecorationPosition position, float rotation, uint objectId) : base(position, rotation,
            objectId)
        {
        }
    }

    public class RoadDecoration
    {
        public DecorationPosition Position { get; }

        public RoadDecoration(DecorationPosition position) => (Position) = (position);
    }

    /// <summary>
    /// This is called RoadOccupantWithModel in game but in practice it's only used for bridges!
    ///
    /// It uses the format string "%04x-bridge" with pieceId as the model name.
    /// </summary>
    public class BridgeDecoration
    {
        /// <summary>
        /// The road portion of the bridge.
        /// </summary>
        public RoadDecoration Road;
        public BridgeDecoration(RoadDecoration road) => (Road) = (road);
    }

    public class PropDecoration : DecorationWithObjectId
    {
        public PropDecoration(DecorationPosition position, float rotation, uint propId) : base(position, rotation,
            propId)
        {
        }
    }
}