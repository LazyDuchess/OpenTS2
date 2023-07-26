using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A cShapeRefNode scenegraph block.
    ///
    /// These reference cShape blocks and ...
    /// </summary>
    public class ShapeRefNodeBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0x65245517;
        public const string BLOCK_NAME = "cShapeRefNode";
        public override string BlockName => BLOCK_NAME;

        public ObjectReference[] Shapes { get; }
        public RenderableNode Renderable { get; }

        public float[] MorphChannelWeights { get; }
        public string[] MorphChannelNames { get; }
        public uint ShapeColor { get; }

        public ShapeRefNodeBlock(PersistTypeInfo blockTypeInfo, ObjectReference[] shapes, RenderableNode renderable,
            float[] morphChannelWeights,
            string[] morphChannelNames, uint shapeColor) : base(blockTypeInfo) =>
            (Shapes, Renderable, MorphChannelWeights, MorphChannelNames, ShapeColor) =
            (shapes, renderable, morphChannelWeights, morphChannelNames, shapeColor);
    }

    public class ShapeRefNodeBlockReader : IScenegraphDataBlockReader<ShapeRefNodeBlock>
    {
        public ShapeRefNodeBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var renderable = RenderableNode.Deserialize(reader);

            var shapes = new ObjectReference[reader.ReadUInt32()];
            for (var i = 0; i < shapes.Length; i++)
            {
                shapes[i] = ObjectReference.Deserialize(reader);
            }

            var displayListFlags = reader.ReadUInt32();

            var morphChannelWeights = new float[reader.ReadUInt32()];
            for (var i = 0; i < morphChannelWeights.Length; i++)
            {
                morphChannelWeights[i] = reader.ReadFloat();
            }

            var morphChannelNames = new string[morphChannelWeights.Length];
            for (var i = 0; i < morphChannelNames.Length; i++)
            {
                morphChannelNames[i] = reader.ReadVariableLengthPascalString();
            }

            // This byte array carries the kMaterialFloat1/2/3/4 attributes. These are likely used in the shader for
            // something.
            var byteArrayLength = reader.ReadUInt32();
            var materialFloats = reader.ReadBytes(byteArrayLength);

            var shapeColor = reader.ReadUInt32();

            return new ShapeRefNodeBlock(blockTypeInfo, shapes, renderable, morphChannelWeights, morphChannelNames, shapeColor);
        }
    }

    // These scenegraph blocks are all part of cShapeRef. They currently live here but can be moved out if they're
    // used in places that aren't just cShapeRef.

    /// <summary>
    /// cRenderableNode
    /// </summary>
    public class RenderableNode
    {
        public BoundedNode Bounded { get; }
        public string[] RenderGroups { get; }
        public uint RenderGroupId { get; }
        public bool AddToDisplayList { get; }

        public RenderableNode(BoundedNode bounded, string[] renderGroups, uint renderGroupId, bool addToDisplayList) =>
            (Bounded, RenderGroups, RenderGroupId, AddToDisplayList) =
            (bounded, renderGroups, renderGroupId, addToDisplayList);

        public static RenderableNode Deserialize(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(typeInfo.Name == "cRenderableNode");

            var bounded = BoundedNode.Deserialize(reader);

            var partOfAllRenderGroups = reader.ReadByte() != 0;
            var renderGroups = new string[reader.ReadUInt32()];
            for (var i = 0; i < renderGroups.Length; i++)
            {
                renderGroups[i] = reader.ReadVariableLengthPascalString();
            }

            var renderGroupId = reader.ReadUInt32();
            // kAddToDisplayListMaskBit
            var addToDisplayList = reader.ReadByte() != 0;

            return new RenderableNode(bounded, renderGroups, renderGroupId, addToDisplayList);
        }
    }

    /// <summary>
    /// cBoundedNode, a node that has a bounding box.
    /// </summary>
    public class BoundedNode
    {
        public TransformNode Transform { get; }

        public BoundedNode(TransformNode transform) => (Transform) = (transform);

        public static BoundedNode Deserialize(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(typeInfo.Name == "cBoundedNode");

            var transform = TransformNode.Deserialize(reader);

            // ignored boolean, always written as 0.
            var ignoredBool = reader.ReadByte();

            return new BoundedNode(transform);
        }
    }

    /// <summary>
    /// cTransformNode, contains transformation and rotation data for a node.
    /// </summary>
    public class TransformNode
    {
        public CompositionTreeNodeBlock CompositionTree { get; }
        public Vector3 Transform { get; }
        public Quaternion Rotation { get; }
        public uint BoneId { get; }

        public TransformNode(CompositionTreeNodeBlock tree, Vector3 transform, Quaternion rotation, uint boneId) =>
            (CompositionTree, Transform, Rotation, BoneId) = (tree, transform, rotation, boneId);

        public static TransformNode Deserialize(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(typeInfo.Name == "cTransformNode");

            var compositionTree = CompositionTreeNodeBlock.Deserialize(reader);

            var transform = Vector3Serializer.Deserialize(reader);
            var rotation = QuaterionSerialzier.Deserialize(reader);
            var boneId = reader.ReadUInt32();
            return new TransformNode(compositionTree, transform, rotation, boneId);
        }
    }
}