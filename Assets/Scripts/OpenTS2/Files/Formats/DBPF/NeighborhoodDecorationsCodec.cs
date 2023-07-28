using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.NHOOD_DECORATIONS)]
    public class NeighborhoodDecorationsCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var decorationsVersion = reader.ReadUInt32();
            Debug.Assert(decorationsVersion >= 108);

            // Flora occupants first.
            var flora = ReadFlora(reader);
            // then roads.
            var roads = ReadRoads(reader);
            // then roads with models.
            var bridges = ReadBridges(reader);
            // and finally prop decorations.
            var props = ReadProps(reader);

            return new NeighborhoodDecorationsAsset(flora, roads, bridges, props);
        }

        private static DecorationPosition ReadDecorationPositionWithoutRotation(IoBuffer reader)
        {
            var removeBoundingYValues = reader.ReadByte() < 2;

            // Can't use Vector3Serializer because this is stored in the order y, x, z for some reason.
            var y = reader.ReadFloat();
            var x = reader.ReadFloat();
            var z = reader.ReadFloat();
            var pos = new Vector3(y, z, x);
            var boundingBoxMin = Vector2Serializer.Deserialize(reader);
            var boundingBoxMax = Vector2Serializer.Deserialize(reader);
            var position = new DecorationPosition
                { Position = pos, BoundingBoxMin = boundingBoxMin, BoundingBoxMax = boundingBoxMax };

            if (removeBoundingYValues)
            {
                position.BoundingBoxMin.y = float.MinValue;
                position.BoundingBoxMax.y = float.MinValue;
            }

            return position;
        }

        private static FloraDecoration[] ReadFlora(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            // Versions above this are read differently (e.g they don't have individual objectVersion fields inside)
            // but don't seem to exist in practice.
            Debug.Assert(version > 7);

            var count = reader.ReadUInt32();
            var flora = new FloraDecoration[count];

            for (var i = 0; i < count; i++)
            {
                var position = ReadDecorationPositionWithoutRotation(reader);
                var objectVersion = reader.ReadByte();
                // Same as above, there is code to handle versions below this in the game but these don't seem to exist
                // and don't even have a rotation or GUID.
                Debug.Assert(objectVersion > 7);

                var rotation = reader.ReadFloat();

                var objectId = reader.ReadUInt32();
                flora[i] = new FloraDecoration(position, rotation, objectId);
            }

            return flora;
        }

        private static RoadDecoration ReadRoad(IoBuffer reader)
        {
            var position = ReadDecorationPositionWithoutRotation(reader);
            var objectVersion = reader.ReadByte();
            Debug.Assert(objectVersion >= 3);

            var corners = new Vector3[4];
            for (var i = 0; i < 4; i++)
            {
                var y = reader.ReadFloat();
                var x = reader.ReadFloat();
                var z = reader.ReadFloat();
                corners[i] = new Vector3(y, z, x);
                // Unknown
                Vector2Serializer.Deserialize(reader);
            }

            var pieceId = reader.ReadUInt32();
            var underTextureId = reader.ReadUInt32();

            var flags = reader.ReadByte();
            var connectionFlag = reader.ReadByte();

            var numberOfAddons = reader.ReadUInt32();
            for (var i = 0; i < numberOfAddons; i++)
            {
                reader.ReadUInt32();
                // Unknown
                Vector3Serializer.Deserialize(reader);
                // Unknown
                Vector2Serializer.Deserialize(reader);
            }

            return new RoadDecoration(position, corners, pieceId, underTextureId, flags, connectionFlag);
        }

        private static RoadDecoration[] ReadRoads(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version > 2);

            var roads = new RoadDecoration[reader.ReadUInt32()];
            for (var i = 0; i < roads.Length; i++)
            {
                roads[i] = ReadRoad(reader);
            }
            return roads;
        }

        private static BridgeDecoration[] ReadBridges(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version > 2);

            var bridges = new BridgeDecoration[reader.ReadUInt32()];
            for (var i = 0; i < bridges.Length; i++)
            {
                var road = ReadRoad(reader);
                var objectVersion = reader.ReadByte();

                var positionOffset = Vector3Serializer.Deserialize(reader);
                var modelOrientation = QuaterionSerialzier.Deserialize(reader);
                // unknown
                var unknown = Vector3Serializer.Deserialize(reader);

                if (objectVersion < 2)
                {
                    positionOffset.x *= 1.25f;
                    positionOffset.y *= 1.25f;
                    unknown.x *= 1.25f;
                    unknown.y *= 1.25f;
                }

                bridges[i] = new BridgeDecoration(road, positionOffset, modelOrientation);
            }

            return bridges;
        }

        private static PropDecoration[] ReadProps(IoBuffer reader)
        {
            var version = reader.ReadUInt16();
            Debug.Assert(version >= 7);

            var propDecorations = new PropDecoration[reader.ReadUInt32()];
            for (var i = 0; i < propDecorations.Length; i++)
            {
                var position = ReadDecorationPositionWithoutRotation(reader);
                var objectVersion = reader.ReadByte();
                Debug.Assert(objectVersion > 7);

                var propId = reader.ReadUInt32();
                var rotation = reader.ReadFloat();

                propDecorations[i] = new PropDecoration(position, rotation, propId);
            }

            return propDecorations;
        }
    }
}