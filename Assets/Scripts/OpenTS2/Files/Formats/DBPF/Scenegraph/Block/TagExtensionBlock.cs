using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    public class TagExtensionBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0x9a809646;
        public const string BLOCK_NAME = "cTagExtension";
        public override string BlockName => BLOCK_NAME;

        public string Tag { get; }

        public TagExtensionBlock(PersistTypeInfo blockTypeInfo, string tag) :
            base(blockTypeInfo) => (Tag) = (tag);
    }

    public class TagExtensionBlockReader : IScenegraphDataBlockReader<TagExtensionBlock>
    {
        public TagExtensionBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var extensionBlockTypeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(extensionBlockTypeInfo.Name == "cExtension", "Block in cExtensionTag was not cExtension");
            var tag = reader.ReadPascalString();
            return new TagExtensionBlock(blockTypeInfo, tag);
        }
    }
}