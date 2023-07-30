using System;
using System.Collections.Generic;
using System.Linq;
using OpenTS2.Common;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block;
using OpenTS2.Files.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph
{
    /// <summary>
    /// This is the base RCOL/Scenegraph Resource Collection.
    /// </summary>
    public class ScenegraphResourceCollection
    {
        /// <summary>
        /// The list of data blocks contained in the resource collection.
        /// </summary>
        public List<ScenegraphDataBlock> Blocks { get; }

        /// <summary>
        ///
        /// </summary>
        public ResourceKey[] FileLinks { get; private set; }

        private ScenegraphResourceCollection()
        {
            Blocks = new List<ScenegraphDataBlock>();
        }

        /// <summary>
        /// Returns the ScenegraphDataBlock inside this resource collection that has type T. Throws an exception if
        /// there isn't any block of type T or if there are multiple.
        /// </summary>
        public T GetBlockOfType<T>() where T : ScenegraphDataBlock
        {
            return (T)Blocks.Single(block => block is T);
        }

        public override string ToString()
        {
            return ScenegraphJsonDumper.DumpCollection(this);
        }

        /// <summary>
        /// Deserializes a ScenegraphResourceCollection containing only a single data block of type T.
        /// </summary>
        public static T DeserializeSingletonScenegraphBlock<T>(IoBuffer reader) where T : ScenegraphDataBlock
        {
            var collection = Deserialize(reader);
            if (collection.Blocks.Count != 1)
            {
                throw new Exception($"Expected single Scenegraph block, got {collection.Blocks.Count}");
            }

            var block = collection.Blocks[0];
            if (!(block is T dataBlock))
            {
                throw new Exception($"Wanted single block of type {typeof(T)}, got {block.GetType()}");
            }
            return dataBlock;
        }


        private const uint ScenegraphHeader = 0xFFFF0001;

        public static ScenegraphResourceCollection Deserialize(IoBuffer reader)
        {
            var collection = new ScenegraphResourceCollection();

            var versionMark = reader.ReadUInt32();
            if (versionMark != ScenegraphHeader)
            {
                throw new Exception("Scenegraph resource has invalid header: " + versionMark.ToString("X"));
            }

            var numFileLinks = reader.ReadUInt32();
            collection.FileLinks = new ResourceKey[numFileLinks];
            for (var i = 0; i < numFileLinks; i++)
            {
                collection.FileLinks[i] = ReadResourceKey(reader);
            }

            var itemCount = reader.ReadUInt32();
            var itemTypes = new uint[itemCount];
            for (var i = 0; i < itemCount; i++)
            {
                itemTypes[i] = reader.ReadUInt32();
            }

            foreach (var itemType in itemTypes)
            {
                var typeInfo = PersistTypeInfo.Deserialize(reader);
                Debug.Assert(typeInfo.TypeId == itemType, "Type info header does not match types list");

                if (!BlockReaders.TryGetValue(itemType, out var factory))
                {
                    throw new NotImplementedException($"Unimplemented Scenegraph type {typeInfo.Name} ({itemType:X})");
                }

                var block = factory.Deserialize(reader, typeInfo);
                Debug.Assert(block.BlockName == typeInfo.Name,
                    "Deserialized block's type name does not match expected");
                collection.Blocks.Add(block);
            }

            return collection;
        }

        // Used to create instances of ScenegraphDataBlock based on the type id.
        private static readonly Dictionary<uint, IScenegraphDataBlockReader<ScenegraphDataBlock>> BlockReaders =
            new Dictionary<uint, IScenegraphDataBlockReader<ScenegraphDataBlock>>()
            {
                { TagExtensionBlock.TYPE_ID, new TagExtensionBlockReader() },
                { ImageDataBlock.TYPE_ID, new ImageDataBlockReader() },
                { MipLevelInfoBlock.TYPE_ID, new MipLevelInfoBlockReader() },
                { GeometryDataContainerBlock.TYPE_ID, new GeometryDataContainerBlockReader() },
                { GeometryNodeBlock.TYPE_ID, new GeometryNodeBlockReader() },
                { ShapeBlock.TYPE_ID, new ShapeBlockReader() },
                { MaterialDefinitionBlock.TYPE_ID, new MaterialDefinitionBlockReader() },
                { ResourceNodeBlock.TYPE_ID, new ResourceNodeBlockReader() },
                { DataListExtensionBlock.TYPE_ID, new DataListExtensionBlockReader() },
                { ShapeRefNodeBlock.TYPE_ID, new ShapeRefNodeBlockReader() },
                { TransformNodeBlock.TYPE_ID, new TransformNodeBlockReader() },
            };

        private static ResourceKey ReadResourceKey(IoBuffer reader)
        {
            var groupId = reader.ReadUInt32();
            var instanceId = reader.ReadUInt32();
            var instanceHi = reader.ReadUInt32();
            var typeId = reader.ReadUInt32();
            return new ResourceKey(instanceId, instanceHi, groupId, typeId);
        }
    }
}