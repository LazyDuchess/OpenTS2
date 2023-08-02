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

    /// <summary>
    /// neighborhood-roads-%s-%08x
    /// neighborhood-roads-%08x
    /// </summary>
    public class RoadDecoration
    {
        public DecorationPosition Position { get; }

        public Vector3[] RoadCorners { get; }

        public uint PieceId { get; }
        public uint UnderTextureId { get; }
        public byte Flags { get; }
        public byte ConnectionFlag { get; }

        public RoadDecoration(DecorationPosition position, Vector3[] roadCorners, uint pieceId, uint underTextureId, byte flags, byte connectionFlag) =>
            (Position, RoadCorners, PieceId, UnderTextureId, Flags, ConnectionFlag) = (position, roadCorners, pieceId, underTextureId, flags, connectionFlag);

        public string GetTextureName(string texture)
        {
            return string.Format(texture, $"{(PieceId | 0x04):x8}");
        }
    }

    /// <summary>
    /// This is called RoadOccupantWithModel in game but in practice it's only used for bridges!
    ///
    /// It literally uses the format string "%04x-bridge" with pieceId as the model name. Maybe a holdover from
    /// simcity where it could be used for tunnels and stuff?
    /// </summary>
    public class BridgeDecoration
    {
        /// <summary>
        /// The road portion of the bridge.
        /// </summary>
        public RoadDecoration Road;

        public Vector3 PositionOffset;
        public Quaternion ModelOrientation;

        public BridgeDecoration(RoadDecoration road, Vector3 positionOffset, Quaternion modelOrientation) =>
            (Road, PositionOffset, ModelOrientation) = (road, positionOffset, modelOrientation);

        public string ResourceName => $"{(Road.PieceId >> 0x10):x4}-bridge_cres";
    }

    public class PropDecoration : DecorationWithObjectId
    {
        public PropDecoration(DecorationPosition position, float rotation, uint propId) : base(position, rotation,
            propId)
        {
        }
    }
}