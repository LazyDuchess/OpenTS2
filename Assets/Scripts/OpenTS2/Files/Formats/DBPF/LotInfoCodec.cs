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
            var baseLotInfo = new BaseLotInfo();
            baseLotInfo.Read(reader, true);
            // Back to the outer LotInfo.
            var locationX = reader.ReadUInt32();
            var locationY = reader.ReadUInt32();
            var neighborhoodToLotHeightOffset = reader.ReadFloat();

            var lotId = reader.ReadInt32();
            var currentFrontEdge = reader.ReadByte();
            var lotSkirtPaintName = reader.ReadUint32PrefixedString();

            // TODO: there's more stuff after this

            return new LotInfoAsset(baseLotInfo, lotId, locationX, locationY, neighborhoodToLotHeightOffset, currentFrontEdge);
        }
    }
}