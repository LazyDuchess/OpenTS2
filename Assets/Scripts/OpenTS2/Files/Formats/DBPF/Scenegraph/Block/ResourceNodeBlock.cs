using OpenTS2.Files.Utils;

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

        /// <summary>
        /// Where this resource is stored, if the reference is missing (index of -1) then the current ResourceCollection
        /// has the resource. If this is set, it could be an external reference.
        /// </summary>
        public ObjectReference ResourceLocation { get; }

        /// <summary>
        /// May be null and graph set depending on the resource node.
        /// </summary>
        public CompositionTreeNodeBlock Tree { get; }

        public ObjectGraphNodeBlock Graph { get; }

        public ResourceNodeBlock(PersistTypeInfo blockTypeInfo, string resourceName, ObjectReference resourceLocation, CompositionTreeNodeBlock tree, ObjectGraphNodeBlock graph) : base(blockTypeInfo) =>
            (ResourceName, ResourceLocation, Tree, Graph) = (resourceName, resourceLocation, tree, graph);

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
            CompositionTreeNodeBlock tree = null;
            ObjectGraphNodeBlock graph = null;
            if (hasTreeNode)
            {
                var resource = ScenegraphResource.Deserialize(reader);
                var compositionTree = CompositionTreeNodeBlock.Deserialize(reader);

                resourceName = resource.ResourceName;
                tree = compositionTree;
            }
            else
            {
                graph = ObjectGraphNodeBlock.Deserialize(reader);
                resourceName = graph.Tag;
            }

            var resourceLocation = ObjectReference.Deserialize(reader);
            var skinType = reader.ReadUInt32();

            return new ResourceNodeBlock(blockTypeInfo, resourceName, resourceLocation, tree, graph);
        }
    }
}