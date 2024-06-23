using System;
using System.Data;

namespace NSpeex
{
    public class SpeexHeader
    {

        public int SampleRate { get; set; }
        /// <summary>
        /// <ul>
        /// <li>0=NB </li>
        /// <li>1=WB </li>
        /// <li>2=UWB </li>
        /// </ul>
        /// </summary>
        public BandMode Mode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Channels { get; set; }


        public bool Vbr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Frames { get; set; }

        public int BitRate { get; set; }

        public SpeexHeader(int sampleRate, BandMode mode, int channels, bool vbr, int bitRate, int frames)
        {
            SampleRate = sampleRate;
            Mode = mode;
            Channels = channels;
            Vbr = vbr;
            this.BitRate = bitRate;
            Frames = frames;
        }

        /// <summary>
        /// build data 80 bytes
        /// </summary>
        /// <returns></returns>
        public byte[] BuildData()
        {
            byte[] data = new byte[80];
            LittleEndian.WriteString(data, 0, "Speex   ");// 0 -  7: speex_string
            LittleEndian.WriteString(data, 8, "speex-1.0");//  8 - 27: speex_version
            Array.Copy(new byte[11],0,data,17,11);
            LittleEndian.WriteInt(data, 28, 1);// 28 - 31: speex_version_id
            LittleEndian.WriteInt(data, 32, 80);// 32 - 35: header_size
            LittleEndian.WriteInt(data, 36, SampleRate);// 36 - 39: rate
            LittleEndian.WriteInt(data, 40, (int)Mode);//40 - 43: mode (0=NB, 1=WB, 2=UWB)
            LittleEndian.WriteInt(data, 44, 4);// 44 - 47: mode_bitstream_version
            LittleEndian.WriteInt(data, 48, Channels);//48 - 51: nb_channels 1,2
            LittleEndian.WriteInt(data, 52, BitRate);//// 52 - 55: bitrate
            LittleEndian.WriteInt(data, 56, 160 << (int)Mode);// 56 - 59: frame_size (NB=160, WB=320, UWB=640)
            LittleEndian.WriteInt(data, 60, Vbr ? 1 : 0);//60 - 63: vbr
            LittleEndian.WriteInt(data, 64, Frames);//64 - 67: frames_per_packet
            LittleEndian.WriteInt(data,68,0);//68 - 71: extra_headers
            LittleEndian.WriteInt(data, 72, 0);//72 - 75: reserved1
            LittleEndian.WriteInt(data, 76, 0);//76 - 79: reserved2
            return data;
        }

    }
}
