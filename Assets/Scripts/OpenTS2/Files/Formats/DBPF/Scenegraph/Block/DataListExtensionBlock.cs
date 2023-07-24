using System;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A cDataListExtension scenegraph block.
    /// </summary>
    public class DataListExtensionBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0x6A836D56;
        public const string BLOCK_NAME = "cDataListExtension";
        public override string BlockName => BLOCK_NAME;

        public DataListExtensionBlock(PersistTypeInfo blockTypeInfo) : base(blockTypeInfo)
        {
        }
    }

    public class DataListExtensionBlockReader : IScenegraphDataBlockReader<DataListExtensionBlock>
    {
        public DataListExtensionBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var cExtension = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(cExtension.Name == "cExtension");
            DeserializeSingleExtensionItem(reader);

            return new DataListExtensionBlock(blockTypeInfo);
        }

        // TODO: store these instead of just reading.
        private static void DeserializeSingleExtensionItem(IoBuffer reader)
        {
            var dataType = reader.ReadByte();
            var name = reader.ReadVariableLengthPascalString();

            switch (dataType)
            {
                case 1:
                    // bool
                    var value = reader.ReadByte() != 0;
                    break;
                case 2:
                    // sint32
                    reader.ReadInt32();
                    break;
                case 3:
                    // float32
                    reader.ReadFloat();
                    break;
                case 4:
                    // Vec2
                    Vector2Serializer.Deserialize(reader);
                    break;
                case 5:
                    // Vec3
                    Vector3Serializer.Deserialize(reader);
                    break;
                case 6:
                    // string
                    reader.ReadVariableLengthPascalString();
                    break;
                case 7:
                    // ExtensionItems list
                    var numItems = reader.ReadUInt32();
                    for (var i = 0; i < numItems; i++)
                    {
                        DeserializeSingleExtensionItem(reader);
                    }
                    break;
                case 8:
                    // quanterion
                    reader.ReadBytes(sizeof(float) * 4);
                    break;
                case 9:
                    // bytes
                    var len = reader.ReadUInt32();
                    var data = reader.ReadBytes(len);
                    break;
                default:
                    throw new ArgumentException($"Unknown type code in data list extension: {dataType}");
            }
        }
    }
}