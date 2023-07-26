using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// cTransformNode, contains transformation and rotation data for a geometry node (linked by a bone id).
    /// </summary>
    public class TransformNodeBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0x65246462;
        public const string BLOCK_NAME = "cTransformNode";
        public override string BlockName => BLOCK_NAME;

        public CompositionTreeNodeBlock CompositionTree { get; }
        public Vector3 Transform { get; }
        public Quaternion Rotation { get; }
        public uint BoneId { get; }

        public TransformNodeBlock(PersistTypeInfo typeInfo, CompositionTreeNodeBlock tree, Vector3 transform,
            Quaternion rotation, uint boneId) : base(typeInfo) =>
            (CompositionTree, Transform, Rotation, BoneId) = (tree, transform, rotation, boneId);
    }

    public class TransformNodeBlockReader : IScenegraphDataBlockReader<TransformNodeBlock>
    {
        public TransformNodeBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            return DeserializeWithTypeInfo(reader, blockTypeInfo);
        }

        public static TransformNodeBlock DeserializeWithoutTypeInfo(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(typeInfo.Name == "cTransformNode");

            return DeserializeWithTypeInfo(reader, typeInfo);
        }

        private static TransformNodeBlock DeserializeWithTypeInfo(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var compositionTree = CompositionTreeNodeBlock.Deserialize(reader);

            var transform = Vector3Serializer.Deserialize(reader);
            var rotation = QuaterionSerialzier.Deserialize(reader);
            var boneId = reader.ReadUInt32();
            return new TransformNodeBlock(blockTypeInfo, compositionTree, transform, rotation, boneId);
        }
    }
}