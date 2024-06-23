using System;
using System.IO;
using System.Text;

namespace NSpeex
{
    public class OggSpeexWriter2 : AbsAudioWriter
    {
        public const int PACKETS_PER_OGG_PAGE = 250;

        private Stream stream;

        /** Defines the encoder mode (0=NB, 1=WB and 2-UWB). */
        private BandMode mode;
        /** Defines the sampling rate of the audio input. */
        private int sampleRate;
        /** Defines the number of channels of the audio input (1=mono, 2=stereo). */
        private int channels;
        /** Defines the number of frames per speex packet. */
        private int nframes;
        /** Defines whether or not to use VBR (Variable Bit Rate). */
        private bool vbr;

        private int bitRate;

        private int size;
        /** Ogg Stream Serial Number */
        private int streamSerialNumber;
        /** Data buffer */
        private byte[] dataBuffer;
        /** Pointer within the Data buffer */
        private int dataBufferPtr;
        /** Header buffer */
        private byte[] headerBuffer;
        /** Pointer within the Header buffer */
        private int headerBufferPtr;
        /** Ogg Page count */
        private int pageCount;
        /** Speex packet count within an Ogg Page */
        private int packetCount;
        /**
         * Absolute granule position
         * (the number of audio samples from beginning of file to end of Ogg Packet).
         */
        private long granulepos;

        private long packetDataSize = 0;

        private OggSpeexWriter2()
        {
            if (streamSerialNumber == 0)
            {
                streamSerialNumber = new Random().Next();
            }
            dataBuffer = new byte[65565];
            dataBufferPtr = 0;
            headerBuffer = new byte[255];
            headerBufferPtr = 0;
            pageCount = 0;
            packetCount = 0;
            granulepos = 0;
        }

        public OggSpeexWriter2(BandMode mode, int sampleRate, int channels, int nframes, bool vbr, int bitRate)
            : this()
        {
            this.mode = mode;
            this.sampleRate = sampleRate;
            this.channels = channels;
            this.nframes = nframes;
            this.vbr = vbr;
            this.bitRate = bitRate;
        }

        public override void Close()
        {

            Flush(true);
            stream.Close();
        }

        public override void WriteHeader(string comment)
        {
            OggPageHeader pageHeader = new OggPageHeader(OggPageHeader.HEADER_TYPE_BOS, 0, streamSerialNumber, pageCount++, 0, 1, new byte[1] { 80 });
            byte[] pageHeaderData = pageHeader.BuildData();


            SpeexHeader speexHeader = new SpeexHeader(sampleRate, mode, channels, vbr, bitRate, nframes);
            byte[] speexHeaderData = speexHeader.BuildData();

            int chksum = OggCrc2.checksum(0, pageHeaderData, 0, pageHeaderData.Length);
            chksum = OggCrc2.checksum(chksum, speexHeaderData, 0, speexHeaderData.Length);
            LittleEndian.WriteInt(pageHeaderData, 22, chksum);

            //Misc.ShowBytes(pageHeaderData, 0, pageHeaderData.Length);
            //Misc.ShowBytes(speexHeaderData, 0, speexHeaderData.Length);

            stream.Write(pageHeaderData, 0, pageHeaderData.Length);
            stream.Write(speexHeaderData, 0, speexHeaderData.Length);

            //Console.WriteLine("write page header:" + pageHeaderData.Length);
            //Console.WriteLine("write speex header:" + speexHeaderData.Length);
            //comment
            pageHeader = new OggPageHeader(OggPageHeader.HEADER_TYPE_NORMAL, 0, streamSerialNumber, pageCount++, 0, 1, new byte[1] { (byte)(comment.Length + 8) });
            pageHeaderData = pageHeader.BuildData();
            speexHeaderData = new byte[comment.Length + 8];
            LittleEndian.WriteInt(speexHeaderData, 0, comment.Length);
            LittleEndian.WriteString(speexHeaderData, 4, comment);
            LittleEndian.WriteInt(speexHeaderData, 4 + comment.Length, 0);
            chksum = OggCrc2.checksum(0, pageHeaderData, 0, pageHeaderData.Length);
            chksum = OggCrc2.checksum(chksum, speexHeaderData, 0, speexHeaderData.Length);
            LittleEndian.WriteInt(pageHeaderData, 22, chksum);
            stream.Write(pageHeaderData, 0, pageHeaderData.Length);
            stream.Write(speexHeaderData, 0, speexHeaderData.Length);

            //Misc.ShowBytes(pageHeaderData, 0, pageHeaderData.Length);
            //Misc.ShowBytes(speexHeaderData, 0, speexHeaderData.Length);
            //Console.WriteLine("write page header:" + pageHeaderData.Length);
            //Console.WriteLine("write speex comment:" + speexHeaderData.Length);
        }

        public override void WritePackage(byte[] data, int offset, int len)
        {
            if (len <= 0)
            { // nothing to write
                return;
            }
            if (packetCount > PACKETS_PER_OGG_PAGE)
            {
                Flush(false);
            }
            Array.Copy(data, offset, dataBuffer, dataBufferPtr, len);
            dataBufferPtr += len;
            headerBuffer[headerBufferPtr++] = (byte)len;
            packetCount++;
            granulepos += nframes * (mode == BandMode.UltraWide ? 640 : (mode == BandMode.Wide ? 320 : 160));
        }

        public void WritePackage(SpeexPacket  packet)
        {
            WritePackage(packet.Data, 0, packet.Size);
        }

        public override void Open(string path)
        {
            stream = File.Create(path);
            size = 0;
        }

        private void Flush(bool eos)
        {
            OggPageHeader pageHeader = new OggPageHeader((eos ? OggPageHeader.HEADER_TYPE_EOS : OggPageHeader.HEADER_TYPE_NORMAL), granulepos, streamSerialNumber, pageCount++, 0, packetCount, headerBuffer);
            byte[] pageHeaderData = pageHeader.BuildData();
            int chksum = OggCrc2.checksum(0, pageHeaderData, 0, pageHeaderData.Length);
            chksum = OggCrc2.checksum(chksum, dataBuffer, 0, dataBufferPtr);
            LittleEndian.WriteInt(pageHeaderData, 22, chksum);
            stream.Write(pageHeaderData, 0, pageHeaderData.Length);
            stream.Write(dataBuffer, 0, dataBufferPtr);
            packetDataSize += dataBufferPtr;

            dataBufferPtr = 0;
            headerBufferPtr = 0;
            packetCount = 0;
            if (eos)
            {
                Console.WriteLine("total write " + packetDataSize +" packet data int byte");
            }
        }

    }
}
