using System;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block.GeometryData
{
    /* Element ids and their names as from the wiki and game.
    ╔═════════════╦════════════════════════╦══════════════╗
    ║ ID          ║ Wiki Name              ║ Game Name    ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x114113c3  ║ (EP4) VertexID         ║ vertexid     ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x114113cd  ║ (EP4) RegionMask       ║ seamvertexid ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x1c4afc56  ║ Blend Indices          ║              ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x3b83078b  ║ Normals List           ║ norm         ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x3bd70105  ║ Bone Weights           ║ weights      ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x5b830781  ║ Vertices               ║ pos          ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x5c4afc5c  ║ Blend Weights          ║              ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x5cf2cfe1  ║ Morph Vertex Deltas    ║ posdelta     ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x69d92b93  ║ Bump Map Normal Deltas ║ binorm       ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x7c4dee82  ║ Target Indices         ║              ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x89d92ba0  ║ Bump Map Normals       ║ tangent      ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0x9bb38afb  ║ Binormals              ║              ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0xbb8307ab  ║ UV Coordinates         ║ tc           ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0xcb6f3a6a  ║ Normal Morph Deltas    ║ normdelta    ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0xcb7206a1  ║ Colour                 ║ uvdelta      ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0xdb830795  ║ UV Coordinate Deltas   ║ col          ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0xdcf2cfdc  ║ Morph Vertex Map       ║ targetidx    ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0xeb720693  ║ Colour Deltas          ║ coldelta     ║
    ╠═════════════╬════════════════════════╬══════════════╣
    ║ 0xfbd70111  ║ Bone Assignments       ║ blendidx     ║
    ╚═════════════╩════════════════════════╩══════════════╝ */
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
        public static GeometryElement ReadElement(uint elementId, byte[] elementData)
        {
            return elementId switch
            {
                GeometryElementIds.Normals => new NormalElement(elementData),
                GeometryElementIds.Vertices => new VertexElement(elementData),
                GeometryElementIds.Tangents => new TangentElement(elementData),
                GeometryElementIds.UVMap => new UVMapElement(elementData),
                GeometryElementIds.Color => new ColorElement(elementData),
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
}