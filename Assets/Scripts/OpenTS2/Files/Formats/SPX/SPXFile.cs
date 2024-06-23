using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NSpeex;

namespace OpenTS2.Files.Formats.SPX
{
    public class SPXFile
    {
        public byte[] DecompressedData;

        public SPXFile(byte[] data)
        {
            Read(data);
        }

        public void Read(byte[] data)
        {
            var ms = new MemoryStream(data);
            var reader = new BinaryReader(ms);
            //SPX1
            var magic = new string(reader.ReadChars(4));
            if (magic != "SPX1")
            {
                throw new IOException("Not a valid Speex file!");
            }
            //always 1
            var channels = reader.ReadByte();
            var decodedSize = reader.ReadInt32();
            // (always 2, ultra-wideband mode, sampling rate 32khz)
            var speexMode = reader.ReadInt32();
            // samples per frame/decoded frame size (640 samples, or 1280 bytes)
            var samplesPerFrame = reader.ReadInt16();

            var decoder = new SpeexDecoder((BandMode)speexMode, false);
            var writer = new PcmWaveWriter(32000, 1);
            var mw = new MemoryStream();
            writer.Open(mw);
            writer.WriteHeader("");
            var shortArray = new short[samplesPerFrame];
            var byteArray = new byte[samplesPerFrame * 2];
            while (ms.Position < ms.Length)
            {
                var frameSize = reader.ReadByte();
                var frame = reader.ReadBytes(frameSize);
                var decodeSize = decoder.Decode(frame, 0, frameSize, shortArray, 0, false);
                Buffer.BlockCopy(shortArray, 0, byteArray, 0, decodeSize * 2);
                writer.WritePacket(byteArray, 0, decodeSize * 2);
            }
            writer.Close();
            reader.Dispose();
            ms.Dispose();
            DecompressedData = mw.ToArray();
            mw.Dispose();
        }
    }
}
