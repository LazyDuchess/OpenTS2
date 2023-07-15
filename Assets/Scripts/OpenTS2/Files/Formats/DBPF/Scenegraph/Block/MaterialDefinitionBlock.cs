using System;
using System.Collections.Generic;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    public class MaterialDefinitionBlock : ScenegraphDataBlock
    {
        public const uint TYPE_ID = 0x49596978;
        public const string BLOCK_NAME = "cMaterialDefinition";
        public override string BlockName => BLOCK_NAME;

        public ScenegraphResource Resource { get; }

        /// <summary>
        /// A dictionary mapping material properties such as "reflectivity" -> "0.5" and
        /// "stdMatCullMode" -> "cullClockwise".
        /// </summary>
        public Dictionary<string, string> MaterialProperties { get; }

        /// <summary>
        /// A list of textures used by the material.
        /// </summary>
        public string[] TextureNames { get; }

        public MaterialDefinitionBlock(PersistTypeInfo blockTypeInfo, ScenegraphResource resource,
            Dictionary<string, string> materialProperties, string[] textureNames) : base(blockTypeInfo)
            => (Resource, MaterialProperties, TextureNames) = (resource, materialProperties, textureNames);
    }

    public class MaterialDefinitionBlockReader : IScenegraphDataBlockReader<MaterialDefinitionBlock>
    {
        public MaterialDefinitionBlock Deserialize(IoBuffer reader, PersistTypeInfo blockTypeInfo)
        {
            // TODO: there's some special handling needed for versions < 8, not implemented yet and probably not
            // necessary if there's no files of that version.
            Debug.Assert(blockTypeInfo.Version >= 8);

            var resource = ScenegraphResource.Deserialize(reader);
            var materialName = reader.ReadVariableLengthPascalString();
            var definitionName = reader.ReadVariableLengthPascalString();

            var numberOfProperties = reader.ReadInt32();
            var properties = new Dictionary<string, string>(numberOfProperties);
            for (var i = 0; i < numberOfProperties; i++)
            {
                properties[reader.ReadVariableLengthPascalString()] = reader.ReadVariableLengthPascalString();
            }

            var numberOfTextures = reader.ReadUInt32();
            var textures = new string[numberOfTextures];
            for (var i = 0; i < numberOfTextures; i++)
            {
                textures[i] = reader.ReadVariableLengthPascalString();
            }

            return new MaterialDefinitionBlock(blockTypeInfo, resource, properties, textures);
        }
    }
}