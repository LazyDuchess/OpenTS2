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
            MeshGeometry staticBounds, MeshGeometry[] bonesBounds) : base(blockTypeInfo)
            => (Resource, Elements, StaticBounds, BonesBounds) = (resource, elements, staticBounds, bonesBounds);
    }

    public class GeometryDataContainerBlockReader : IScenegraphDataBlockReader<GeometryDataContainerBlock>
    {
        public GeometryDataContainerBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var resource = ScenegraphResource.Deserialize(reader);
            var elements = ReadElementsSection(reader, blockTypeInfo);

            // Read but ignore the data in the mesh components section for now.
            ReadMeshComponentsSection(reader, blockTypeInfo);
            // Read but ignore the data in the primitives section for now.
            ReadPrimitivesSection(reader, blockTypeInfo);

            var geometry = ReadStaticBoundSection(reader, blockTypeInfo);
            var bones = ReadBonesSection(reader, blockTypeInfo);

            return new GeometryDataContainerBlock(blockTypeInfo, resource, elements, geometry, bones);
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
                reader.ReadUInt32();
                reader.ReadUInt32();

                // This data gets interpreted differently depending
                // on the type of the element.
                var size = reader.ReadUInt32();
                var data = reader.ReadBytes(size);

                var indicesArray = ReadIndices(reader, blockTypeInfo.Version);

                // TODO: for now this is just based on the element id and data, we might need to start parsing the rest.
                elements[i] = GeometryElement.ReadElement(elementId, data);
            }

            return elements;
        }

        private static void ReadMeshComponentsSection(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var numberOfComponents = reader.ReadUInt32();
            Debug.Log($"numberOfMeshComponents: {numberOfComponents}");
            for (var i = 0; i < numberOfComponents; i++)
            {
                var eltArrayIndices = ReadIndices(reader, blockTypeInfo.Version);

                var numVertices = reader.ReadUInt32();
                // marked as number of active elements in simswiki
                reader.ReadUInt32();

                var positionClassIndices = ReadIndices(reader, blockTypeInfo.Version);
                var surfaceClassIndices = ReadIndices(reader, blockTypeInfo.Version);
                var materialClassIndices = ReadIndices(reader, blockTypeInfo.Version);
            }
        }

        private static void ReadPrimitivesSection(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var numberOfPrimitives = reader.ReadUInt32();
            Debug.Log($"numberOfPrimitives: {numberOfPrimitives}");
            for (var i = 0; i < numberOfPrimitives; i++)
            {
                reader.ReadUInt32();
                reader.ReadUInt32();

                var primitiveName = reader.ReadVariableLengthPascalString();
                Debug.Log($"primitive name: {primitiveName}");

                ReadIndices(reader, blockTypeInfo.Version);

                // marked as opacity amount in simswiki
                reader.ReadInt32();

                if (blockTypeInfo.Version <= 1) continue;

                ReadIndices(reader, blockTypeInfo.Version);
            }
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

        private static MeshGeometry ReadStaticBoundSection(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            var numberOfPoseTransforms = reader.ReadUInt32();
            for (var i = 0; i < numberOfPoseTransforms; i++)
            {
                var rotation = QuaterionSerialzier.Deserialize(reader);
                var position = Vector3Serializer.Deserialize(reader);
            }

            var numberOfMorphTargets = reader.ReadUInt32();
            for (var i = 0; i < numberOfMorphTargets; i++)
            {
                reader.ReadVariableLengthPascalString();
                reader.ReadVariableLengthPascalString();
            }

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