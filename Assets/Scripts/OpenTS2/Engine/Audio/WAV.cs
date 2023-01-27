using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine.Audio
{
    /// <summary>
    /// Converts WAV audio data to Unity AudioClip data.
    /// </summary>
    public class WAV
    {
        private int _channels;
        private int _frequency;
        private int _sampleCount;
        private float[] _data;
        public int Channels => _channels;
        public int Frequency => _frequency;
        public int SampleCount => _sampleCount;
        public float[] Data => _data;

        /// <summary>
        /// Creates an AudioClip from provided WAV data.
        /// </summary>
        /// <param name="rawData">Bytes of WAV file.</param>
        /// <param name="name">Name to give the AudioClip.</param>
        /// <returns>An AudioClip.</returns>
        public static AudioClip ToAudioClip(byte[] rawData, string name)
        {
            var wavData = new WAV(rawData);
            AudioClip audioClip = AudioClip.Create(name, wavData.SampleCount, wavData.Channels, wavData.Frequency, false);
            audioClip.SetData(wavData.Data, 0);
            return audioClip;
        }

        /// <summary>
        /// Parses provided raw WAV bytes to construct a WAV instance.
        /// </summary>
        /// <param name="rawData">WAV file bytes.</param>
        public WAV(byte[] rawData)
        {
            var stream = new MemoryStream(rawData);
            var reader = new BinaryReader(stream);
            // RIFF
            reader.ReadBytes(4);
            // RIFF Size
            reader.ReadBytes(4);
            // Wavefmt
            reader.ReadBytes(8);
            // Wavefmt Size
            reader.ReadBytes(4);
            var tag = reader.ReadUInt16();
            _channels = reader.ReadUInt16();
            _frequency = (int)reader.ReadUInt32();
            var byteRate = reader.ReadUInt32();
            var align = reader.ReadUInt16();
            var bitDepth = reader.ReadUInt16();
            // "data"
            reader.ReadBytes(4);
            var dataSize = reader.ReadUInt32();

            var byteSize = 2;

            switch(bitDepth)
            {
                case 8:
                    byteSize = 1;
                    break;
                case 16:
                    byteSize = 2;
                    break;
                case 24:
                    byteSize = 3;
                    break;
                case 32:
                    byteSize = 4;
                    break;
            }

            _sampleCount = (int)(rawData.Length - stream.Position) / byteSize;
            _data = new float[SampleCount];

            var i = 0;
            while (stream.Position < rawData.Length)
            {
                for (var j = 0; j < Channels; j++)
                {
                    switch (byteSize)
                    {
                        case 1:
                            _data[i] = (float)reader.ReadSByte() / sbyte.MaxValue;
                            break;
                        case 2:
                            _data[i] = (float)reader.ReadInt16() / short.MaxValue;
                            break;
                        case 3:
                            _data[i] = (float)BitConverter.ToInt32(reader.ReadBytes(3), 0) / int.MaxValue;
                            break;
                        case 4:
                            _data[i] = (float)reader.ReadInt32() / int.MaxValue;
                            break;
                    }
                    i++;
                }
            }

            _sampleCount /= Channels;
        }
    }
}
