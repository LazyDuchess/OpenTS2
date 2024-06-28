using OpenTS2.Files.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class BaseLotInfo
    {
        public uint Flags { get; private set; }
        public byte LotType { get; private set; }
        public string LotName { get; private set; }
        public string LotDescription { get; private set; }
        public uint Width { get; private set; }
        public uint Depth { get; private set; }
        public byte RoadsAlongEdges { get; private set; }
        public byte CreationFrontEdge { get; private set; }

        public void Read(IoBuffer reader)
        {
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

            Flags = flags;
            LotType = lotType;
            LotName = lotName;
            LotDescription = lotDescription;
            Width = creationWidth;
            Depth = creationDepth;
            RoadsAlongEdges = roadsAlongEdges;
            CreationFrontEdge = creationFrontEdge;
        }
    }
}
