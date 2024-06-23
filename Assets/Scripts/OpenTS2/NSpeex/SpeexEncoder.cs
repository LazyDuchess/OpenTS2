//
// Copyright (C) 2009-2010 Christoph Fröschl
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 
// - Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// 
// - Neither the name of the Xiph.org Foundation nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;

namespace NSpeex
{
	/// <summary>
	/// NSpeex Encoder class. This class encodes the given PCM 16bit samples into speex
    /// frames.
	/// </summary>
	public class SpeexEncoder
	{
		/// <summary>
		/// Version of the Speex Encoder
		/// </summary>
		public const String Version = ".Net Speex Encoder v0.0.1";

		private readonly IEncoder encoder;
		private readonly Bits bits;
		private readonly float[] rawData;
		private readonly int frameSize;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mode">the mode of the encoder (0=NB, 1=WB, 2=UWB).</param>
		/// <returns>true if initialisation successful.</returns>
		public SpeexEncoder(BandMode mode)
		{
			bits = new Bits();
			switch (mode)
			{
				case BandMode.Narrow:
					encoder = new NbEncoder();
					break;
				case BandMode.Wide:
					encoder = new SbEncoder(false);
					break;
				case BandMode.UltraWide:
					encoder = new SbEncoder(true);
					break;
				default:
					throw new ArgumentException("Invalid mode", "mode");
			}

			/* set decoder format and properties */
			frameSize = encoder.FrameSize;
            rawData = new float[frameSize];
		}

        /// <summary>
        /// The sampling rate in samples per second
        /// </summary>
		public int SampleRate
		{
			get
			{
				return encoder.SamplingRate;
			}
		}

        /// <summary>
        /// The encoder quality within the range [0-10].
        /// </summary>
        public int Quality
        {
            set
            {
                encoder.Quality = value;
            }
        }

        /// <summary>
        /// Turns encoding in variable bit rate on or off.
        /// </summary>
        public bool VBR
        {
            get
            {
                return encoder.Vbr;
            }

            set
            {
                encoder.Vbr = value;
            }
        }

        /// <summary>
        /// The frame size indicates the samples which are packed in a single Speex frame.
        /// </summary>
		public int FrameSize
		{
			get
			{
				return frameSize;
			}
		}

        /// <summary>
        /// Encodes the given sample data.
        /// </summary>
        /// <param name="inData">Array of samples.</param>
        /// <param name="inOffset">Start offset for the inData.</param>
        /// <param name="inCount">Number of samples to encode. Must be a multiple of <see cref="FrameSize"/>.</param>
        /// <param name="outData">The encoded data.</param>
        /// <param name="outOffset">Start offset when writing to outData</param>
        /// <param name="outCount">The length of the outData array (maximum number of bytes writting after encoding).</param>
        /// <returns>The bytes encoded.</returns>
        public int Encode(short[] inData, int inOffset, int inCount, byte[] outData, int outOffset, int outCount)
        {
            bits.Reset();
            int samplesProcessed = 0;
            int result = 0;
            while (samplesProcessed < inCount)
            {
                // convert shorts into float samples,
                for (int i = 0; i < frameSize; i++)
                    rawData[i] = inData[inOffset + i + samplesProcessed];

                result += encoder.Encode(bits, rawData);
                samplesProcessed += frameSize;
            }

            if (result == 0)
                return 0;

            return bits.Write(outData, outOffset, outCount);
        }
	}
}
