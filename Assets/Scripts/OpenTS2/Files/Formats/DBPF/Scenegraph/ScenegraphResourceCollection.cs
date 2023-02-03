using System;
using System.Collections.Generic;
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

        private ScenegraphResourceCollection()
        {
            Blocks = new List<ScenegraphDataBlock>();
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

            var fileLinks = reader.ReadUInt32();
            if (fileLinks != 0)
            {
                throw new NotImplementedException("Scenegraph links are not implemented yet");
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
                    throw new NotImplementedException($"Unimplemented Scenegraph type {typeInfo.Name} ({itemType})");
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
                { TagExtensionBlock.TYPE_ID, new TagExtensionBlockReader() }
            };
    }
}