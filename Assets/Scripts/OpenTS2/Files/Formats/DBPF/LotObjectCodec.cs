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
                "cLocomotable" => ReadLocomotableObject(reader, version),
                "cPerson" => ReadPersonObject(reader, version),
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
                    var skinType = reader.ReadUInt32();
                }

                var numOverrides = reader.ReadInt32();
                for (var j = 0; j < numOverrides; j++)
                {
                    var nameMaterialOverride = reader.ReadVariableLengthPascalString();
                    var subsetNameOverride = reader.ReadVariableLengthPascalString();
                }
            }

            var position = Vector3Serializer.Deserialize(reader);
            var rotation = QuaternionSerializer.Deserialize(reader);

            var numBones = reader.ReadUInt32();
            for (var i = 0; i < numBones; i++)
            {
                var boneName = reader.ReadVariableLengthPascalString();
                var bonePosition = Vector3Serializer.Deserialize(reader);
                var boneRotation = QuaternionSerializer.Deserialize(reader);
            }

            if (version > 15)
            {
                var numBlends = reader.ReadUInt32();
                for (var i = 0; i < numBlends; i++)
                {
                    var blendTargetMaybe = reader.ReadVariableLengthPascalString();
                    var blendName = reader.ReadVariableLengthPascalString();
                    var blendValue = reader.ReadFloat();
                }
            }

            return new LotObjectAsset.LotObject(resourceName, position, rotation);
        }

        private static LotObjectAsset.AnimatableObject ReadAnimatableObject(IoBuffer reader, uint version)
        {
            // cAnimatable starts with a cObject.
            var baseObject = ReadObject(reader);

            Debug.Assert(version < 17);
            var skeletonStretch = reader.ReadFloat();

            if (reader.ReadInt32() != 0)
            {
                var objectTransform = Vector3Serializer.Deserialize(reader);
                var objectRotation = QuaternionSerializer.Deserialize(reader);

                var globalTransform = Vector3Serializer.Deserialize(reader);
                var globalRotation = QuaternionSerializer.Deserialize(reader);
            }

            if (version < 15)
            {
                // two choreo requests, we don't implement deserializing these yet.
                Debug.Assert(reader.ReadInt32() == 0);
                Debug.Assert(reader.ReadInt32() == 0);

                reader.ReadInt32();
                reader.ReadInt32();
            }

            var animateSimInstanceCounter = reader.ReadUInt32();
            if (version > 11)
            {
                var newReachInstanceCounter = reader.ReadUInt32();
            }

            if (version < 10)
            {
                reader.ReadUInt32();
            }
            if (version > 8)
            {
                reader.ReadInt32();
            }
            if (version > 10)
            {
                reader.ReadInt32();
            }
            if (version > 12)
            {
                reader.ReadFloat();
            }

            if (version > 13)
            {
                if (version >= 16)
                {
                    reader.ReadUInt32();
                }
                reader.ReadUInt32();
            }

            var unknownVec = Vector3Serializer.Deserialize(reader);
            var unknownQuat = QuaternionSerializer.Deserialize(reader);

            var numberOfAnimRequests = reader.ReadUInt32();
            for (int i = 0; i < numberOfAnimRequests; i++)
            {
                var animRequestType = reader.ReadInt32();
                if (animRequestType == 0)
                {
                    continue;
                }
                // TODO: handle this.
            }

            return new LotObjectAsset.AnimatableObject(baseObject);
        }

        private static LotObjectAsset.LocomotableObject ReadLocomotableObject(IoBuffer reader, uint version)
        {
            // cLocomotable starts with a cAnimatable.
            var animatable = ReadObject(reader);
            Debug.Assert(animatable is LotObjectAsset.AnimatableObject);

            return new LotObjectAsset.LocomotableObject(animatable);
        }

        private static LotObjectAsset.PersonObject ReadPersonObject(IoBuffer reader, uint version)
        {
            // cPerson starts with a cLocomotable.
            var locomotable = ReadObject(reader);
            Debug.Assert(locomotable is LotObjectAsset.LocomotableObject);

            return new LotObjectAsset.PersonObject(locomotable);
        }

    }
}