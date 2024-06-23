using System;

namespace NSpeex
{
    /// <summary>
    /// Ogg Page Header
    /// <ul>
    /// <li> 0 -  3: capture_pattern</li>
    /// <li> 4: stream_structure_version</li>
    /// <li> 5: header_type_flag (0=normal, 2=bos: beginning of stream, 4=eos: end of stream).</li>
    /// <li> 6 - 13: absolute granule position</li>
    /// <li> 14 - 17: stream serial number</li>
    /// <li> 18 - 21: page sequence no</li>
    /// <li> 22 - 25: page checksum</li>
    /// <li> 26: page_segments</li>
    /// <li>  27 -  x: segment_table</li>
    /// </ul>
    /// </summary>
    public class OggPageHeader
    {
        public const int HEADER_TYPE_NORMAL = 0;
        public const int HEADER_TYPE_BOS = 2;
        public const int HEADER_TYPE_EOS = 4;

        public int HeaderType { get; set; }
        public long GranulePos { get; set; }
        public int StreamSerialNumber { get; set; }

        public int PageCount { get; set; }

        public int CheckSum { get; set; }

        public int PacketCount { get; set; }

        public byte[] PacketSizes { get; set; }

        public OggPageHeader(int headerType, long granulePos, int streamSerialNumber, int pageCount, int checkSum, int packetCount, byte[] packetSizes)
        {
            HeaderType = headerType;
            GranulePos = granulePos;
            StreamSerialNumber = streamSerialNumber;
            PageCount = pageCount;
            CheckSum = checkSum;
            PacketCount = packetCount;
            PacketSizes = packetSizes;
        }

        public byte[] BuildData()
        {
            byte[] data = new byte[27+PacketCount];
            LittleEndian.WriteString(data,0,"OggS");//0 -  3: capture_pattern
            data[4] = 0;//4: stream_structure_version
            data[5] = (byte)(HeaderType);//5: header_type_flag
            LittleEndian.WriteLong(data, 6, GranulePos);//6 - 13: absolute granule position
            LittleEndian.WriteInt(data, 14, StreamSerialNumber);//14 - 17: stream serial number
            LittleEndian.WriteInt(data, 18, PageCount);//18 - 21: page sequence no
            LittleEndian.WriteInt(data, 22, CheckSum);//22 - 25: page checksum
            data[26] = (byte)PacketCount; //     26: page_segments
            Array.Copy(PacketSizes,0,data,27,PacketCount);
            return data;
        }

    }
}
