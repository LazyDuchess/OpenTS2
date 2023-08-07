using System;
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
        public RenderableNodeBlock Renderable { get; }

        public float[] MorphChannelWeights { get; }
        public string[] MorphChannelNames { get; }
        public uint ShapeColor { get; }

        public ShapeRefNodeBlock(PersistTypeInfo blockTypeInfo, ObjectReference[] shapes, RenderableNodeBlock renderable,
            float[] morphChannelWeights,
            string[] morphChannelNames, uint shapeColor) : base(blockTypeInfo) =>
            (Shapes, Renderable, MorphChannelWeights, MorphChannelNames, ShapeColor) =
            (shapes, renderable, morphChannelWeights, morphChannelNames, shapeColor);
    }

    public class ShapeRefNodeBlockReader : IScenegraphDataBlockReader<ShapeRefNodeBlock>
    {
        public ShapeRefNodeBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var renderable = RenderableNodeBlock.Deserialize(reader);

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

            var morphChannelNames = Array.Empty<string>();
            if (blockTypeInfo.Version >= 21)
            {
                morphChannelNames = new string[morphChannelWeights.Length];
                for (var i = 0; i < morphChannelNames.Length; i++)
                {
                    morphChannelNames[i] = reader.ReadVariableLengthPascalString();
                }
            }

            // This byte array carries the kMaterialFloat1/2/3/4 attributes. These are likely used in the shader for
            // something.
            var byteArrayLength = reader.ReadUInt32();
            var materialFloats = reader.ReadBytes(byteArrayLength);

            uint shapeColor = 0;
            if (blockTypeInfo.Version > 19)
            {
                shapeColor = reader.ReadUInt32();
            }

            return new ShapeRefNodeBlock(blockTypeInfo, shapes, renderable, morphChannelWeights, morphChannelNames, shapeColor);
        }
    }
}