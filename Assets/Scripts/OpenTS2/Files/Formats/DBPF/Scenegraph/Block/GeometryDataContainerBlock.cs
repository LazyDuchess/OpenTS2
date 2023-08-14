using System.Collections.Generic;
using System.Linq;
using OpenTS2.Files.Formats.DBPF.Scenegraph.Block.GeometryData;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// Contains the vertices and faces of a mesh.
    /// </summary>
    public struct MeshGeometry
    {
        public Vector3[] Vertices;
        public ushort[] Faces;
    }

    /// <summary>
    /// "Linkages" on the wiki. This ties together a bunch of `GeometryElement`s together as a single component.
    /// Primitives, known as "groups" on the wiki index into these components.
    /// </summary>
    public struct MeshComponent
    {
        public ushort[] GeometryElementIndices;

        /// <summary>
        /// If non-zero length, defines which indices in the mesh's vertex map to use for this component.
        /// </summary>
        public ushort[] VertexAliases;

        /// <summary>
        /// If non-zero length, defines which indices in the normal map to use for this component.
        /// </summary>
        public ushort[] NormalMapAliases;

        /// <summary>
        /// If non-zero length, defines which indices in the uv map to use for this component.
        /// </summary>
        public ushort[] UVMapAliases;
    }

    /// <summary>
    /// Called "groups" on the wiki. These are named parts of the model that may be independent of one another such
    /// as the bedding vs the frame of a bed model.
    /// </summary>
    public struct MeshPrimitive
    {
        public string Name;
        public uint ComponentIndex;
        public ushort[] Faces;
        /// <summary>
        /// Translates from the local bone assignment numbers to the "real" bone indices in the GMDC.
        /// </summary>
        public ushort[] BoneIndices;
    }

    public class GeometryDataContainerBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0xAC4F8687;
        public const string BLOCK_NAME = "cGeometryDataContainer";
        public override string BlockName => BLOCK_NAME;

        public ScenegraphResource Resource { get; }

        /// <summary>
        /// The elements contain the bulk of the data about the model such as the normal maps, vertices, uv maps etc.
        /// </summary>
        public GeometryElement[] Elements { get; }

        public MeshComponent[] Components { get; }
        public MeshPrimitive[] Primitives { get; }
        public BindPose[] BindPoses { get; }

        public MorphTarget[] MorphTargets { get; }

        /// <summary>
        /// Static bounding mesh for the whole model. Only used when there are no joints/bones.
        /// </summary>
        public MeshGeometry StaticBounds { get; }

        /// <summary>
        /// Bounding meshes for the different bones of the model.
        /// </summary>
        public MeshGeometry[] BonesBounds { get; }

        public GeometryDataContainerBlock(PersistTypeInfo blockTypeInfo,
            ScenegraphResource resource, GeometryElement[] elements,
            MeshComponent[] components, MeshPrimitive[] primitives, BindPose[] bindPoses, MorphTarget[] morphTargets,
            MeshGeometry staticBounds, MeshGeometry[] bonesBounds) : base(blockTypeInfo)
        {
            Resource = resource;
            Elements = elements;
            Components = components;
            Primitives = primitives;
            BindPoses = bindPoses;
            MorphTargets = morphTargets;
            StaticBounds = staticBounds;
            BonesBounds = bonesBounds;
        }

        public MeshComponent GetMeshComponentForPrimitive(MeshPrimitive primitive)
        {
            return Components[primitive.ComponentIndex];
        }

        public List<GeometryElement> GetGeometryElementsForMeshComponent(MeshComponent component)
        {
            return component.GeometryElementIndices.Select(elementIndex => Elements[elementIndex]).ToList();
        }

        public readonly struct MorphTarget
        {
            public readonly string groupName;
            public readonly string channelName;

            public MorphTarget(string group, string channel) => (groupName, channelName) = (group, channel);
        }

        public readonly struct BindPose
        {
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;

            public BindPose(Vector3 position, Quaternion rotation) => (Position, Rotation) = (position, rotation);
        }
    }

    public class GeometryDataContainerBlockReader : IScenegraphDataBlockReader<GeometryDataContainerBlock>
    {
        public GeometryDataContainerBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var resource = ScenegraphResource.Deserialize(reader);
            var elements = ReadElementsSection(reader, blockTypeInfo);

            var components = ReadMeshComponentsSection(reader, blockTypeInfo);
            var primitives = ReadPrimitivesSection(reader, blockTypeInfo);

            var bindPoses = ReadPoseTransforms(reader);
            var morphTargets = ReadMorphTargets(reader);
            var staticBound = ReadStaticBoundSection(reader, blockTypeInfo);
            var bones = ReadBonesSection(reader, blockTypeInfo);

            return new GeometryDataContainerBlock(blockTypeInfo, resource, elements, components, primitives,
                bindPoses, morphTargets, staticBound, bones);
        }

        private static ushort[] ReadIndices(IoBuffer reader, uint version)
        {
            var indicesArray = new ushort[reader.ReadUInt32()];
            for (var j = 0; j < indicesArray.Length; j++)
            {
                indicesArray[j] = (version < 3) switch
                {
                    true => (ushort)reader.ReadInt32(),
                    _ => reader.ReadUInt16()
                };
            }

            return indicesArray;
        }

        private static GeometryElement[] ReadElementsSection(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var numberOfElements = reader.ReadUInt32();
            var elements = new GeometryElement[numberOfElements];

            for (var i = 0; i < numberOfElements; i++)
            {
                reader.ReadUInt32();
                var elementId = reader.ReadUInt32();
                reader.ReadUInt32();
                var elementFormat = reader.ReadUInt32();
                reader.ReadUInt32();

                // This data gets interpreted differently depending
                // on the type of the element.
                var size = reader.ReadUInt32();
                var data = reader.ReadBytes(size);

                var indicesArray = ReadIndices(reader, blockTypeInfo.Version);

                // TODO: for now this is just based on the element id and data, we might need to start parsing the rest.
                elements[i] = GeometryElement.ReadElement(elementId, data, elementFormat);
            }

            return elements;
        }

        // These are called mesh components in the game and linkages on the wiki. They tie together sets of elements
        // from the geometry elements.
        private static MeshComponent[] ReadMeshComponentsSection(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var numberOfComponents = reader.ReadUInt32();
            var components = new MeshComponent[numberOfComponents];
            for (var i = 0; i < numberOfComponents; i++)
            {
                var eltArrayIndices = ReadIndices(reader, blockTypeInfo.Version);

                var numVertices = reader.ReadUInt32();
                // marked as number of active elements in simswiki, in practice will probably always match the
                // number of element array indexes we have.
                var numEltArrayIndices = reader.ReadUInt32();
                Debug.Assert(numEltArrayIndices == eltArrayIndices.Length);

                var vertexIndicesAliases = ReadIndices(reader, blockTypeInfo.Version);
                var normalIndicesAliases = ReadIndices(reader, blockTypeInfo.Version);
                var uvMapIndicesAliases = ReadIndices(reader, blockTypeInfo.Version);

                components[i] = new MeshComponent
                {
                    GeometryElementIndices = eltArrayIndices, VertexAliases = vertexIndicesAliases,
                    NormalMapAliases = normalIndicesAliases, UVMapAliases = uvMapIndicesAliases
                };
            }

            return components;
        }

        private static MeshPrimitive[] ReadPrimitivesSection(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var numberOfPrimitives = reader.ReadUInt32();
            var primitives = new MeshPrimitive[numberOfPrimitives];

            for (var i = 0; i < numberOfPrimitives; i++)
            {
                var primitiveType = reader.ReadUInt32();
                var componentIndex = reader.ReadUInt32();

                var primitiveName = reader.ReadVariableLengthPascalString();

                var faces = ReadIndices(reader, blockTypeInfo.Version);

                // TODO: this is actually the draw order for transparent objects. Lower values here mean the object
                // should be drawn first. This is used for example in hot tubs where the shadow is drawn first, then
                // the water, then the foam.
                //   marked as opacity amount in simswiki
                reader.ReadInt32();

                Debug.Assert(blockTypeInfo.Version > 1);
                var remappedBoneIndices = ReadIndices(reader, blockTypeInfo.Version);

                primitives[i] = new MeshPrimitive
                    { Name = primitiveName, ComponentIndex = componentIndex, Faces = faces, BoneIndices = remappedBoneIndices };
            }

            return primitives;
        }

        private static MeshGeometry ReadGeometry(IoBuffer reader, uint version)
        {
            var numberOfVertices = reader.ReadUInt32();
            if (numberOfVertices == 0)
            {
                return new MeshGeometry();
            }

            var numberOfFacesIndices = reader.ReadUInt32();
            Debug.Assert(numberOfFacesIndices % 3 == 0, "face indices not divisible by 3");

            var vertices = new Vector3[numberOfVertices];
            for (var i = 0; i < numberOfVertices; i++)
            {
                vertices[i] = Vector3Serializer.Deserialize(reader);
            }

            var faces = new ushort[numberOfFacesIndices];
            for (var i = 0; i < numberOfFacesIndices; i++)
            {
                faces[i] = version < 4 ? (ushort)reader.ReadUInt32() : reader.ReadUInt16();
            }

            return new MeshGeometry { Vertices = vertices, Faces = faces };
        }

        private static GeometryDataContainerBlock.BindPose[] ReadPoseTransforms(IoBuffer reader)
        {
            var bindPoses = new GeometryDataContainerBlock.BindPose[reader.ReadUInt32()];
            for (var i = 0; i < bindPoses.Length; i++)
            {
                var rotation = QuaterionSerialzier.Deserialize(reader);
                var position = Vector3Serializer.Deserialize(reader);
                bindPoses[i] = new GeometryDataContainerBlock.BindPose(position, rotation);
            }

            return bindPoses;
        }

        private static GeometryDataContainerBlock.MorphTarget[] ReadMorphTargets(IoBuffer reader)
        {
            var numberOfMorphTargets = reader.ReadUInt32();
            var morphTargets = new GeometryDataContainerBlock.MorphTarget[numberOfMorphTargets];
            for (var i = 0; i < numberOfMorphTargets; i++)
            {
                var groupName = reader.ReadVariableLengthPascalString();
                var channelName = reader.ReadVariableLengthPascalString();
                morphTargets[i] = new GeometryDataContainerBlock.MorphTarget(groupName, channelName);
            }

            return morphTargets;
        }

        private static MeshGeometry ReadStaticBoundSection(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            return ReadGeometry(reader, blockTypeInfo.Version);
        }

        private static MeshGeometry[] ReadBonesSection(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var numberOfBones = reader.ReadUInt32();

            var bones = new MeshGeometry[numberOfBones];
            for (var i = 0; i < numberOfBones; i++)
            {
                bones[i] = ReadGeometry(reader, blockTypeInfo.Version);
            }

            return bones;
        }
    }
}