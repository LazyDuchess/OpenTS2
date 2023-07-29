using System;
using System.Collections.Generic;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// A scenegraph cShape block.
    ///
    /// These contain references to multiple `GeometryNodeBlock`s and their associated `MaterialDefinitionBlock`s
    /// and stitch together multiple meshes to make one complete object.
    ///
    /// For example, the cShape for the crashed ufo in strangetown has 3 subsets: the body of the ufo, the glass cabin
    /// and the pre-baked shadow. These all have different textures/materials.
    /// </summary>
    public class ShapeBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0xFC6EB1F7;
        public const string BLOCK_NAME = "cShape";
        public override string BlockName => BLOCK_NAME;

        public ScenegraphResource Resource { get; }

        /// <summary>
        /// The list of Level-of-Details supported by the shape.
        /// </summary>
        public int[] LodLevels { get; }

        /// <summary>
        /// Keys are LoD levels and the value contains the meshes to be rendered at that LoD.
        /// </summary>
        public Dictionary<uint, IList<string>> MeshesPerLod { get; }

        /// <summary>
        /// Mapping of GMDC group names to their materials.
        /// </summary>
        public Dictionary<string, string> Materials { get; }

        public ShapeBlock(PersistTypeInfo blockTypeInfo, ScenegraphResource resource, int[] lodLevels,
            Dictionary<uint, IList<string>> meshesPerLod, Dictionary<string, string> materials) : base(blockTypeInfo) =>
            (Resource, LodLevels, MeshesPerLod, Materials) = (resource, lodLevels, meshesPerLod, materials);

        public override string ToString()
        {
            return base.ToString() + " " + Resource.ResourceName + "\n"
                + $"  LodLevels=[{string.Join(", ", LodLevels)}]\n"
                + $"  MeshesPerLod={MeshesPerLod}";
        }
    }

    public class ShapeBlockReader : IScenegraphDataBlockReader<ShapeBlock>
    {
        public static readonly int[] LODLevels = { 0x0F000000 };

        public ShapeBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            // Versions below this are read differently, not sure if they're out in the wild.
            Debug.Assert(blockTypeInfo.Version > 5);

            var resource = ScenegraphResource.Deserialize(reader);

            // This is a cReferentNode, but it's just a cObjectGraphNode wrapped in another block so we didn't bother
            // making a separate class for it.
            var referentType = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(referentType.Name == "cReferentNode");
            var graphNode = ObjectGraphNodeBlock.Deserialize(reader);

            int[] lodLevels = { };
            if (blockTypeInfo.Version > 6)
            {
                lodLevels = new int[reader.ReadUInt32()];
                for (var i = 0; i < lodLevels.Length; i++)
                {
                    lodLevels[i] = reader.ReadInt32();
                }
            }
            // Default lodLevel
            if (lodLevels.Length == 0)
            {
                lodLevels = LODLevels;
            }

            var meshesPerLod = new Dictionary<uint, IList<string>>();
            var numberOfMeshes = reader.ReadUInt32();
            for (var i = 0; i < numberOfMeshes; i++)
            {
                var levelOfDetail = reader.ReadUInt32();

                var hashMeshByReference = true;
                if (blockTypeInfo.Version >= 8)
                {
                    hashMeshByReference = reader.ReadByte() == 0x00;
                }

                // Either the mesh is a reference from the ResourceCollection FileLinks or it just has a mesh name.
                string meshName;
                if (hashMeshByReference)
                {
                    // throw here for now, most shape blocks don't use this and we'd need some more code to add two
                    // different types of fields for this in the `ShapeBlock` class.
                    var mesh = ObjectReference.Deserialize(reader);
                    throw new ArgumentException("meshes referenced in FileLinks not supported");
                }
                else
                {
                    meshName = reader.ReadVariableLengthPascalString();
                }

                if (!meshesPerLod.ContainsKey(levelOfDetail))
                {
                    meshesPerLod[levelOfDetail] = new List<string>();
                }
                meshesPerLod[levelOfDetail].Add(meshName);
            }

            // Links each Primitive in the GMDC to a material name.
            var materials = new Dictionary<string, string>();
            var materialBinds = reader.ReadUInt32();
            for (var i = 0; i < materialBinds; i++)
            {
                var groupName = reader.ReadVariableLengthPascalString();
                var materialName = reader.ReadVariableLengthPascalString();
                materials[groupName] = materialName;

                // The wiki claims this render group stuff is deprecated, leave it unused for now.
                var addToDefaultRenderGroup = reader.ReadByte() != 0;
                var additionalRenderGroups = new string[reader.ReadUInt32()];
                for (var j = 0; j < additionalRenderGroups.Length; j++)
                {
                    additionalRenderGroups[i] = reader.ReadVariableLengthPascalString();
                }

                var renderGroupId = reader.ReadUInt32();
            }

            return new ShapeBlock(blockTypeInfo, resource, lodLevels, meshesPerLod, materials);
        }
    }
}