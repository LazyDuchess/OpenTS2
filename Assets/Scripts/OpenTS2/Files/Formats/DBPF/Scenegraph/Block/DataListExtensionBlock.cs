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

        public DataListValue Value { get; }

        public DataListExtensionBlock(PersistTypeInfo blockTypeInfo, DataListValue value) : base(blockTypeInfo)
        {
            Value = value;
        }
    }

    public abstract class DataListValue
    {
        public string Name;

        public DataListValue(string name) => (Name) = (name);
    }

    public class DataListValue<T> : DataListValue
    {
        public T Value { get; }

        public DataListValue(string name, T value) : base(name) => (Value) = (value);
    }

    public class DataListExtensionBlockReader : IScenegraphDataBlockReader<DataListExtensionBlock>
    {
        public DataListExtensionBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var cExtension = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(cExtension.Name == "cExtension");
            var value = DeserializeSingleExtensionItem(reader);

            return new DataListExtensionBlock(blockTypeInfo, value);
        }

        // TODO: store these instead of just reading.
        private static DataListValue DeserializeSingleExtensionItem(IoBuffer reader)
        {
            var dataType = reader.ReadByte();
            var name = reader.ReadVariableLengthPascalString();

            switch (dataType)
            {
                case 1:
                    // bool
                    var value = reader.ReadByte() != 0;
                    return new DataListValue<bool>(name, value);
                case 2:
                    // sint32
                    return new DataListValue<int>(name, reader.ReadInt32());
                case 3:
                    // float32
                    return new DataListValue<float>(name, reader.ReadFloat());
                case 4:
                    // Vec2
                    return new DataListValue<Vector2>(name, Vector2Serializer.Deserialize(reader));
                case 5:
                    // Vec3
                    return new DataListValue<Vector3>(name, Vector3Serializer.Deserialize(reader));
                case 6:
                    // string
                    return new DataListValue<string>(name, reader.ReadVariableLengthPascalString());
                case 7:
                    // ExtensionItems list
                    var items = new DataListValue[reader.ReadUInt32()];
                    for (var i = 0; i < items.Length; i++)
                    {
                        items[i] = DeserializeSingleExtensionItem(reader);
                    }
                    return new DataListValue<DataListValue[]>(name, items);
                case 8:
                    // quanterion
                    return new DataListValue<Quaternion>(name, QuaterionSerialzier.Deserialize(reader));
                case 9:
                    // bytes
                    var len = reader.ReadUInt32();
                    var data = reader.ReadBytes(len);
                    return new DataListValue<byte[]>(name, data);
                default:
                    throw new ArgumentException($"Unknown type code in data list extension: {dataType}");
            }
        }
    }
}