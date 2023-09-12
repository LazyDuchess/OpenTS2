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

        public int HeadingOffsetAxis { get; }
        public int MirrorAxis { get; }
        public int StretchAxis { get; }
        public float StretchFactor { get; }
        public Quaternion JointOrientation { get; }


        public BoneDataExtensionBlock(PersistTypeInfo blockTypeInfo, int headingOffsetAxis, int mirrorAxis,
            int stretchAxis, float stretchFactor, Quaternion jointOrientation) : base(blockTypeInfo)
        {
            HeadingOffsetAxis = headingOffsetAxis;
            MirrorAxis = mirrorAxis;
            StretchAxis = stretchAxis;
            StretchFactor = stretchFactor;
            JointOrientation = jointOrientation;
        }
    }

    public class BoneDataExtensionBlockReader : IScenegraphDataBlockReader<BoneDataExtensionBlock>
    {
        public BoneDataExtensionBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            // Starts with a cExtension.
            var extensionBlockTypeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(extensionBlockTypeInfo.Name == "cExtension",
                "First block in cBoneDataExtension was not cExtension");

            var headingOffsetAxis = reader.ReadInt32();
            var mirrorAxis = reader.ReadInt32();
            var stretchAxis = reader.ReadInt32();
            var stretchFactor = reader.ReadFloat();

            Quaternion jointOrientation = Quaternion.identity;
            if (blockTypeInfo.Version > 3)
            {
                jointOrientation = QuaternionSerializer.Deserialize(reader);
            }

            return new BoneDataExtensionBlock(blockTypeInfo, headingOffsetAxis, mirrorAxis, stretchAxis, stretchFactor,
                jointOrientation);
        }
    }
}