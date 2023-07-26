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

            return new NeighborhoodDecorationsAsset(flora);
        }

        private static DecorationPosition ReadDecorationPosition(IoBuffer reader)
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
                var position = ReadDecorationPosition(reader);
                var objectVersion = reader.ReadByte();
                // Same as above, there is code to handle versions below this in the game but these don't seem to exist
                // and don't even have a rotation or GUID.
                Debug.Assert(objectVersion > 7);

                var rotation = reader.ReadFloat();
                position.Rotation = rotation;

                var objectId = reader.ReadUInt32();
                flora[i] = new FloraDecoration(position, objectId);
            }

            return flora;
        }
    }
}