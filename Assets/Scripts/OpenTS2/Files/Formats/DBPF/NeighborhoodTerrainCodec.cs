using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    /// <summary>
    /// Neighborhood geometry/terrain file reading codec.
    /// </summary>
    [Codec(TypeIDs.NHOOD_TERRAIN)]
    public class NeighborhoodTerrainCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var classId = reader.ReadUInt32();
            if (classId != TypeIDs.NHOOD_TERRAIN)
            {
                throw new ArgumentException($"Neighborhood terrain id not {TypeIDs.NHOOD_TERRAIN:X}");
            }

            var version = reader.ReadUInt32();

            var width = reader.ReadInt32();
            var height = reader.ReadInt32();

            var seaLevel = reader.ReadFloat();
            
            // Versions below this don't actually carry the terrain data! Don't think those exist
            // in the wild so just throw an exception if we encounter one of them.
            if (version <= 2)
            {
                throw new ArgumentException($"Neighorhood terrain version too old {version}");
            }


            var terrainType = reader.ReadUint32PrefixedString();
            var vertexHeights = FloatArray2D.Deserialize(reader);

            return new NeighborhoodTerrainAsset(width, height, seaLevel, terrainType, vertexHeights);
        }
    }
}