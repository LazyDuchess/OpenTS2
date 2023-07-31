using System;
using System.IO;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF
{
    [Codec(TypeIDs.LOT_INFO)]
    public class LotInfoCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var stream = new MemoryStream(bytes);
            var reader = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN);

            var version = reader.ReadUInt16();
            // Not sure if versions below this are in the wild but they need different reading code.
            if (version < 12)
            {
                throw new NotSupportedException("LotInfo assets below version 12 not supported");
            }

            // Inside the lotInfo is a nested baseLotInfo that carries information about the original lot.
            var baseLotInfoVersion = reader.ReadUInt16();
            var creationWidth = reader.ReadUInt32();
            var creationDepth = reader.ReadUInt32();
            var lotType = reader.ReadByte();
            var roadsAlongEdges = reader.ReadByte();
            var creationFrontEdge = reader.ReadByte();
            var flags = (baseLotInfoVersion < 5) ? 0 : reader.ReadUInt32();
            var lotName = "";
            var lotDescription = "";
            if (baseLotInfoVersion > 5)
            {
                lotName = reader.ReadUint32PrefixedString();
                lotDescription = reader.ReadUint32PrefixedString();
            }
            if (baseLotInfoVersion > 3)
            {
                var lotHeights = new float[reader.ReadUInt32()];
                for (var i = 0; i < lotHeights.Length; i++)
                {
                    lotHeights[i] = reader.ReadFloat();
                }
            }
            if (baseLotInfoVersion > 6)
            {
                var creationRoadHeight = reader.ReadFloat();
            }
            if (baseLotInfoVersion > 7)
            {
                // unknown
                reader.ReadUInt32();
            }
            if (baseLotInfoVersion == 11)
            {
                var apartmentCount = reader.ReadByte();
                var apartmentRentalPriceHigh = reader.ReadUInt32();
                var apartmentRentalPriceLow = reader.ReadUInt32();
                var lotClass = reader.ReadUInt32();
                var overrideLotClass = reader.ReadByte();
            }

            // Back to the outer LotInfo.
            var locationX = reader.ReadUInt32();
            var locationY = reader.ReadUInt32();
            var neighborhoodToLotHeightOffset = reader.ReadFloat();

            var lotId = reader.ReadInt32();
            var currentFrontEdge = reader.ReadByte();
            var lotSkirtPaintName = reader.ReadUint32PrefixedString();

            // TODO: there's more stuff after this

            return new LotInfoAsset(lotId, flags, lotType, lotName, lotDescription,
                creationWidth, creationDepth, roadsAlongEdges, locationX, locationY, neighborhoodToLotHeightOffset,
                creationFrontEdge, currentFrontEdge);
        }
    }
}