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
    /// NSpeex Decoder class. This class decodes the given speex frames into
    /// PCM 16bit samples.
    /// </summary>
	public class SpeexDecoder
	{
		private readonly int sampleRate;
		private float[] decodedData;
		private readonly Bits bits;
		private readonly IDecoder decoder;
		private readonly int frameSize;

		/// <summary>
		/// Constructor
        /// <param name="mode">The mode of the decoder.</param>
        /// <param name="enhanced">Whether to enable perceptual enhancement or not.</param>
		/// </summary>
        public SpeexDecoder(BandMode mode, bool enhanced = true)
        {
            bits = new Bits();
            switch (mode)
            {
                case BandMode.Narrow: 
                    decoder = new NbDecoder();
                    sampleRate = 8000;
                    break;
                // Wideband
                case BandMode.Wide: 
                    decoder = new SbDecoder(false);
                    sampleRate = 16000;
                    break;
                case BandMode.UltraWide: 
                    decoder = new SbDecoder(true);
                    sampleRate = 32000;
                    break;
                // */
                default: 
                    decoder = new NbDecoder();
                    sampleRate = 8000;
                    break;
            }

            /* initialize the speex decoder */
            decoder.PerceptualEnhancement = enhanced;
            /* set decoder format and properties */
            frameSize = decoder.FrameSize;
            decodedData = new float[sampleRate * 2]; // init buffer to 1 second
		}

        /// <summary>
        /// The frame size indicates the samples which are packed in a single Speex frame.
        /// </summary>
        public int FrameSize
        {
            get
            {
                return decoder.FrameSize;
            }
        }

        /// <summary>
        /// The sampling rate in samples per second
        /// </summary>
		public int SampleRate
		{
			get
			{
				return sampleRate;
			}
		}

        /// <summary>
        /// Decodes the given encoded data.
        /// </summary>
        /// <param name="inData">The encoded data. Can be multiple frames.</param>
        /// <param name="inOffset">Start offset where to read the encoded data from.</param>
        /// <param name="inCount">The number of bytes to decode.</param>
        /// <param name="outData">The output of the decoded data in samples.</param>
        /// <param name="outOffset">Start offset where to start writing the decoded samples from.</param>
        /// <param name="lostFrame">Indicates if we are decoding a lost frame. Alternatively the <paramref name="inData"/> parameter can be <value>null</value>.</param>
        /// <returns>The number of samples decoded.</returns>
        public int Decode(byte[] inData, int inOffset, int inCount, short[] outData, int outOffset, bool lostFrame)
        {
            if (decodedData.Length < outData.Length * 2)
            {
                // resize the decoded data buffer
                decodedData = new float[outData.Length * 2];
            }

            if (lostFrame || inData == null)
            {
                decoder.Decode(null, decodedData);
                for (int i = 0; i < frameSize; i++, outOffset++)
                {
                    outData[outOffset] = ConvertToShort(decodedData[i]);
                }
                return frameSize;
            }

            bits.ReadFrom(inData, inOffset, inCount);
            int samplesDecoded = 0;
            while (decoder.Decode(bits, decodedData) == 0)
            {
                for (int i = 0; i < frameSize; i++, outOffset++)
                {

                    outData[outOffset] = ConvertToShort(decodedData[i]);
                }
                samplesDecoded += frameSize;
            }

            return samplesDecoded;
        }


        private static short ConvertToShort(float value)
        {
            // PCM saturation
            if (value > 32767.0f)
                value = 32767.0f;
            else if (value < -32768.0f)
                value = -32768.0f;

            // Convert to short and save to buffer
            return (short)Math.Round(value);
        }
	}
}
