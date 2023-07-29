using System;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A sccenegraph cResourceNode block.
    /// </summary>
    public class ResourceNodeBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0xE519C933;
        public const string BLOCK_NAME = "cResourceNode";
        public override string BlockName => BLOCK_NAME;

        public string ResourceName { get; }

        public ResourceNodeBlock(PersistTypeInfo blockTypeInfo, string resourceName) : base(blockTypeInfo) =>
            (ResourceName) = (resourceName);

        public override string ToString()
        {
            return $"{base.ToString()}  ResourceName={ResourceName}";
        }
    }

    public class ResourceNodeBlockReader : IScenegraphDataBlockReader<ResourceNodeBlock>
    {
        public ResourceNodeBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var hasTreeNode = reader.ReadByte() != 0;

            string resourceName;
            if (hasTreeNode)
            {
                var resource = ScenegraphResource.Deserialize(reader);
                var compositionTree = CompositionTreeNodeBlock.Deserialize(reader);

                resourceName = resource.ResourceName;
            }
            else
            {
                var graph = ObjectGraphNodeBlock.Deserialize(reader);
                resourceName = graph.Tag;
            }

            var reference = ObjectReference.Deserialize(reader);
            var skinType = reader.ReadUInt32();

            return new ResourceNodeBlock(blockTypeInfo, resourceName);
        }
    }
}