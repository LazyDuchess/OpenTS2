using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF.Scenegraph;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_OBJECT)]
    public class LotObjectCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            // Skip 64 bytes.
            reader.Seek(SeekOrigin.Current, 64);

            return new LotObjectAsset(ReadObject(reader));
        }

        private static LotObjectAsset.LotObject ReadObject(IoBuffer reader)
        {
            var typeId = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            var blockName = reader.ReadVariableLengthPascalString();

            return blockName switch
            {
                "cObject" => ReadBaseObject(reader, version),
                "cAnimatable" => ReadAnimatableObject(reader, version),
                _ => throw new NotImplementedException($"Invalid LotObject type {blockName}")
            };
        }

        private static LotObjectAsset.LotObject ReadBaseObject(IoBuffer reader, uint version)
        {
            var resourceName = reader.ReadVariableLengthPascalString();

            // This part is called scenegraph "skins" in game. Not sure what it's quite for yet.
            var numSkins = reader.ReadInt32();
            for (var i = 0; i < numSkins; i++)
            {
                var skinName = reader.ReadVariableLengthPascalString();
                if (version > 16)
                {
                    reader.ReadUInt32(); // unknown
                }

                var unknownPairCount = reader.ReadInt32();
                for (var j = 0; j < unknownPairCount; j++)
                {
                    var str1 = reader.ReadVariableLengthPascalString();
                    var str2 = reader.ReadVariableLengthPascalString();
                }
            }

            var position = Vector3Serializer.Deserialize(reader);
            var rotation = QuaterionSerialzier.Deserialize(reader);

            var numBones = reader.ReadUInt32();
            for (var i = 0; i < numBones; i++)
            {
                var boneName = reader.ReadVariableLengthPascalString();
                var bonePosition = Vector3Serializer.Deserialize(reader);
                var boneRotation = QuaterionSerialzier.Deserialize(reader);
            }

            return new LotObjectAsset.LotObject(resourceName, position, rotation);
        }

        private static LotObjectAsset.AnimatableObject ReadAnimatableObject(IoBuffer reader, uint version)
        {
            // cAnimatable starts with a cObject.
            var baseObject = ReadObject(reader);

            return new LotObjectAsset.AnimatableObject(baseObject);
        }

    }
}