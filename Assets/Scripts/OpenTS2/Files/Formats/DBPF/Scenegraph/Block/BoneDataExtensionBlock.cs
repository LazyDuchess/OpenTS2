using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    public class BoneDataExtensionBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0xE9075BC5;
        public const string BLOCK_NAME = "cBoneDataExtension";
        public override string BlockName => BLOCK_NAME;

        public BoneDataExtensionBlock(PersistTypeInfo blockTypeInfo) : base(blockTypeInfo)
        {
        }
    }

    public class BoneDataExtensionBlockReader : IScenegraphDataBlockReader<BoneDataExtensionBlock>
    {
        public BoneDataExtensionBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            // Starts with a cExtension.
            var extensionBlockTypeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(extensionBlockTypeInfo.Name == "cExtension", "First block in cBoneDataExtension was not cExtension");

            var headingOffsetAxis = reader.ReadInt32();
            var mirrorAxis = reader.ReadInt32();
            var stretchAxis = reader.ReadInt32();
            var stretchFactor = reader.ReadFloat();

            if (blockTypeInfo.Version > 3)
            {
                var jointOrientation = QuaterionSerialzier.Deserialize(reader);
            }

            return new BoneDataExtensionBlock(blockTypeInfo);
        }
    }
}