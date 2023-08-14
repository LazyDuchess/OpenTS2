using System;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block.GeometryData
{
    /* Element ids and their names as from the wiki and game. Parenthesized names are my reverse-engineered names from
       the game.

    ╔═════════════╦════════════════════════╦══════════════════╗
    ║ ID          ║ Wiki Name              ║ Game Name        ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x114113c3  ║ (EP4) VertexID         ║ vertexid         ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x114113cd  ║ (EP4) RegionMask       ║ seamvertexid     ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x1c4afc56  ║ Blend Indices          ║ (pos_delta_idx)  ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x3b83078b  ║ Normals List           ║ norm             ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x3bd70105  ║ Bone Weights           ║ weights          ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x5b830781  ║ Vertices               ║ pos              ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x5c4afc5c  ║ Blend Weights          ║                  ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x5cf2cfe1  ║ Morph Vertex Deltas    ║ posdelta         ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x69d92b93  ║ Bump Map Normal Deltas ║ binorm           ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x7c4dee82  ║ Target Indices         ║ (norm_delta_idx) ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x89d92ba0  ║ Bump Map Normals       ║ tangent          ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0x9bb38afb  ║ Binormals              ║                  ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0xbb8307ab  ║ UV Coordinates         ║ tc               ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0xcb6f3a6a  ║ Normal Morph Deltas    ║ normdelta        ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0xcb7206a1  ║ Colour                 ║ uvdelta          ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0xdb830795  ║ UV Coordinate Deltas   ║ col              ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0xdcf2cfdc  ║ Morph Vertex Map       ║ targetidx        ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0xeb720693  ║ Colour Deltas          ║ coldelta         ║
    ╠═════════════╬════════════════════════╬══════════════════╣
    ║ 0xfbd70111  ║ Bone Assignments       ║ blendidx         ║
    ╚═════════════╩════════════════════════╩══════════════════╝ */
    internal static class GeometryElementIds
    {
        public const uint Normals = 0x3b83078b;
        public const uint Vertices = 0x5b830781;
        public const uint Tangents = 0x89d92ba0;
        public const uint UVMap = 0xbb8307ab;
        /// <summary>
        /// The wiki claims this is a "UV Coordinate Delta" but the game seems to have three sets of these elements and
        /// they're called `color`. Each element seems to be an `unsigned long` with a packed ARGB color value.
        /// </summary>
        public const uint Color = 0xdb830795;

        /// <summary>
        /// How much each vertex should move during a morph animation.
        /// </summary>
        public const uint MorphVertexPositionDelta = 0x5cf2cfe1;
        /// <summary>
        /// Contains the vertex indices that have vertex position deltas, useless extra information.
        /// </summary>
        public const uint MorphVertexPositionIndices = 0x1c4afc56;
        /// <summary>
        /// How much the normal vectors should move during a morph animation.
        /// </summary>
        public const uint MorphNormalDelta = 0xcb6f3a6a;
        /// <summary>
        /// Contains the vertex indices that have normal deltas, useless extra information.
        /// </summary>
        public const uint MorphNormalIndices = 0x7c4dee82;
        /// <summary>
        /// This is a bit of a special structure, it has a uint32 for each vertex. It maps out which
        /// MorphVertexPositionDelta and MorphNormalDelta blocks should affect the vertex and which morphTarget the
        /// morph is considered part of.
        ///
        /// See SceneGraphModelAsset.AddBlendAnimations for more details.
        /// </summary>
        public const uint MorphVertexMap = 0xdcf2cfdc;

        /// <summary>
        /// One uint32 per vertex. Each 8-bit value in the uint32 represents an assigned bone index. A value of 255
        /// means no bone is assigned.
        /// </summary>
        public const uint BoneAssignments = 0xfbd70111;
        /// <summary>
        /// Depending on the number of bones per vertex this can be either a list of floats, a list of two floats or a
        /// list of three floats. The floats should add up to 1.0 and represent the weight for the corresponding bone
        /// from the BoneAssignments.
        /// </summary>
        public const uint BoneWeights = 0x3bd70105;
    }

    /// <summary>
    /// GeometryElements are basic containers of geometric data such as uv maps, vertex lists, normal vectors etc.
    ///
    /// These are stored separately so they can be reused in different groups of the same model. For example, a model
    /// for a bed may have groups for the frame, bedding and shadow that use different sets of faces in the
    /// VertexElement or share a UVMapElement.
    /// </summary>
    public abstract class GeometryElement
    {
        public static GeometryElement ReadElement(uint elementId, byte[] elementData, uint elementFormat)
        {
            return elementId switch
            {
                GeometryElementIds.Normals => new NormalElement(elementData),
                GeometryElementIds.Vertices => new VertexElement(elementData),
                GeometryElementIds.Tangents => new TangentElement(elementData),
                GeometryElementIds.UVMap => new UVMapElement(elementData),
                GeometryElementIds.Color => new ColorElement(elementData),

                GeometryElementIds.MorphVertexPositionDelta => new MorphVertexPositionDeltaElement(elementData),
                GeometryElementIds.MorphVertexPositionIndices => new MorphVertexPositionIndicesElement(elementData),
                GeometryElementIds.MorphNormalDelta => new MorphNormalDeltaElement(elementData),
                GeometryElementIds.MorphNormalIndices => new MorphNormalIndicesElement(elementData),
                GeometryElementIds.MorphVertexMap => new MorphVertexMapElement(elementData),

                GeometryElementIds.BoneAssignments => new BoneAssignmentsElement(elementData),

                GeometryElementIds.BoneWeights when elementFormat == 0 => new BoneWeightsForSingleBonesElement(elementData),
                GeometryElementIds.BoneWeights when elementFormat == 1 => new BoneWeightsForTwoBonesElement(elementData),
                GeometryElementIds.BoneWeights => new BoneWeightsForThreeBonesElement(elementData),

                _ => throw new ArgumentException($"Unknown geometry element id {elementId:X}")
            };
        }
    }

    /// <summary>
    /// A GeometryElement consisting of 3 float elements: used for vertices, normal maps etc.
    /// </summary>
    public abstract class Vec3Element : GeometryElement
    {
        public Vector3[] Data { get; }

        protected Vec3Element(byte[] elementData)
        {
            var numElements = elementData.Length / (3 * sizeof(float));
            Data = new Vector3[numElements];

            var buffer = IoBuffer.FromBytes(elementData);
            for (var i = 0; i < numElements; i++)
            {
                Data[i] = Vector3Serializer.Deserialize(buffer);
            }
        }
    }

    /// <summary>
    /// A GeometryElement consisting of 2 float elements: used for uv maps, uv deltas etc.
    /// </summary>
    public abstract class Vec2Element : GeometryElement
    {
        public Vector2[] Data { get; }

        protected Vec2Element(byte[] elementData)
        {
            var numElements = elementData.Length / (2 * sizeof(float));
            Data = new Vector2[numElements];

            var buffer = IoBuffer.FromBytes(elementData);
            for (var i = 0; i < numElements; i++)
            {
                Data[i] = Vector2Serializer.Deserialize(buffer);
            }
        }
    }

    /// <summary>
    /// A GeometryElement consisting of floats.
    /// </summary>
    public abstract class FloatElement : GeometryElement
    {
        public float[] Data { get; }

        protected FloatElement(byte[] elementData)
        {
            var numElements = elementData.Length / sizeof(float);
            Data = new float[numElements];

            var buffer = IoBuffer.FromBytes(elementData);
            for (var i = 0; i < numElements; i++)
            {
                Data[i] = buffer.ReadFloat();
            }
        }
    }

    /// <summary>
    /// A GeometryElement consisting of unsigned 32-bit values.
    /// </summary>
    public abstract class UnsignedInt32Element : GeometryElement
    {
        public uint[] Data { get; }

        protected UnsignedInt32Element(byte[] elementData)
        {
            var numElements = elementData.Length / sizeof(uint);
            Data = new uint[numElements];

            var buffer = IoBuffer.FromBytes(elementData);
            for (var i = 0; i < numElements; i++)
            {
                Data[i] = buffer.ReadUInt32();
            }
        }
    }

    /// <summary>
    /// A GeometryElement consisting of unsigned shorts, usually used to represent vertex indices.
    /// </summary>
    public abstract class UnsignedInt16Element : GeometryElement
    {
        public ushort[] Data { get; }

        protected UnsignedInt16Element(byte[] elementData)
        {
            var numElements = elementData.Length / sizeof(ushort);
            Data = new ushort[numElements];

            var buffer = IoBuffer.FromBytes(elementData);
            for (var i = 0; i < numElements; i++)
            {
                Data[i] = buffer.ReadUInt16();
            }
        }
    }

    public class VertexElement : Vec3Element
    {
        public VertexElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class NormalElement : Vec3Element
    {
        public NormalElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class TangentElement : Vec3Element
    {
        public TangentElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class UVMapElement : Vec2Element
    {
        public UVMapElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class ColorElement : UnsignedInt32Element
    {
        public ColorElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class MorphVertexPositionDeltaElement : Vec3Element
    {
        public MorphVertexPositionDeltaElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class MorphVertexPositionIndicesElement : UnsignedInt16Element
    {
        public MorphVertexPositionIndicesElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class MorphNormalDeltaElement : Vec3Element
    {
        public MorphNormalDeltaElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class MorphNormalIndicesElement : UnsignedInt16Element
    {
        public MorphNormalIndicesElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class MorphVertexMapElement : UnsignedInt32Element
    {
        public MorphVertexMapElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class BoneAssignmentsElement : UnsignedInt32Element
    {
        public BoneAssignmentsElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public interface IBoneWeightsElement
    {

    }

    public class BoneWeightsForSingleBonesElement : FloatElement, IBoneWeightsElement
    {
        public BoneWeightsForSingleBonesElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class BoneWeightsForTwoBonesElement : Vec2Element, IBoneWeightsElement
    {
        public BoneWeightsForTwoBonesElement(byte[] elementData) : base(elementData)
        {
        }
    }

    public class BoneWeightsForThreeBonesElement : Vec3Element, IBoneWeightsElement
    {
        public BoneWeightsForThreeBonesElement(byte[] elementData) : base(elementData)
        {
        }
    }
}