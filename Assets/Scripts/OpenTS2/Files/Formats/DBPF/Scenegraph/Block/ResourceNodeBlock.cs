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

        public ScenegraphResource Resource { get; }

        public ResourceNodeBlock(PersistTypeInfo blockTypeInfo, ScenegraphResource resource) : base(blockTypeInfo) =>
            (Resource) = (resource);
    }

    public class ResourceNodeBlockReader : IScenegraphDataBlockReader<ResourceNodeBlock>
    {
        public ResourceNodeBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var hasTreeNode = reader.ReadByte() != 0;

            ScenegraphResource resource;
            if (hasTreeNode)
            {
                resource = ScenegraphResource.Deserialize(reader);
                var compositionTree = CompositionTreeNodeBlock.Deserialize(reader);
            }
            else
            {
                // TODO: we store ScenegraphResource above, figure out what to do when it's not available.
                throw new NotImplementedException("cResourceNode without composition tree not supported");
                var graph = ObjectGraphNodeBlock.Deserialize(reader);
            }

            var reference = ObjectReference.Deserialize(reader);
            var skinType = reader.ReadUInt32();

            return new ResourceNodeBlock(blockTypeInfo, resource);
        }
    }
}