//
// Copyright (C) 2003 Jean-Marc Valin
// Copyright (C) 1999-2003 Wimba S.A., All Rights Reserved.
// Copyright (C) 2008 Filip Navara
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

using System.IO;
using System;

namespace NSpeex
{
	/// <summary>
	/// Writes basic PCM wave files from binary audio data.
	/// 
	/// Here's an example that writes 2 seconds of silence
	/// 
	/// PcmWaveWriter s_wsw = new PcmWaveWriter(2, 44100);
	/// byte[] silence = new byte[16/// 2/// 44100];
	/// wsw.Open(&quot;C:\\out.wav&quot;);
	/// wsw.WriteHeader();
	/// wsw.WriteData(silence, 0, silence.length);
	/// wsw.WriteData(silence, 0, silence.length);
	/// wsw.Close();
	/// </summary>
	public class PcmWaveWriter : AudioFileWriter
	{
		/// <summary>
		/// Wave type code of PCM
		/// </summary>
		protected const short WAVE_FORMAT_PCM = (short)0x01;

		/// <summary>
		/// Wave type code of Speex
		/// </summary>
		protected const short WAVE_FORMAT_SPEEX = unchecked((short)0xa109);

		/// <summary>
		/// Table describing the number of frames per packet in a Speex Wave file,
		/// depending on its mode (0=NB, 1=WB, 3=UWB), channels-1 (1=mono,
		/// 2=stereo) and the quality setting (0 to 10). See end of file for exerpt
		/// from SpeexACM code for more explanations.
		/// </summary>
		public static readonly int[,,] WAVE_FRAME_SIZES = new int[,,]
		{
			{
				{ 8, 8, 8, 1, 1, 2, 2, 2, 2, 2, 2 }, // NB mono
				{ 2, 1, 1, 7, 7, 8, 8, 8, 8, 3, 3 }  // NB stereo
			},
			{
				{ 8, 8, 8, 2, 1, 1, 2, 2, 2, 2, 2 }, // WB mono
				{ 1, 2, 2, 8, 7, 6, 3, 3, 3, 3, 3 }  // WB stereo
			},
			{
				{ 8, 8, 8, 1, 2, 2, 1, 1, 1, 1, 1 }, // UWB mono
				{ 2, 1, 1, 7, 8, 3, 6, 6, 5, 5, 5 }  // UWB stereo
			}
		};

		/// <summary>
		/// Table describing the number of bit per Speex frame, depending on its
		/// mode (0=NB, 1=WB, 2=UWB), channels-1 (1=mono, 2=stereo) and the quality
		/// setting (0 to 10). See end of file for exerpt from SpeexACM code for more
		/// explanations.
		/// </summary>
		public static readonly int[,,] WAVE_BITS_PER_FRAME = new int[,,]
		{
			{
				{ 43, 79, 119, 160, 160, 220, 220, 300, 300, 364, 492 }, // NB mono
				{ 60, 96, 136, 177, 177, 237, 237, 317, 317, 381, 509 }  // NB stereo
			}, 
			{
				{ 79, 115, 155, 196, 256, 336, 412, 476, 556, 684, 844 }, // WB mono
				{ 96, 132, 172, 213, 273, 353, 429, 493, 573, 701, 861 }  // WB stereo
			},
			{
				{ 83, 151, 191, 232, 292, 372, 448, 512, 592, 720, 880 }, // UWB mono
				{ 100, 168, 208, 249, 309, 389, 465, 529, 609, 737, 897 } // UWB stereo
			}
		};

		private BinaryWriter raf;

		/// <summary>
		/// Defines the encoder mode (0=NB, 1=WB and 2-UWB).
		/// </summary>
		private readonly int mode;
		private readonly int quality;

		/// <summary>
		/// Defines the sampling rate of the audio input.
		/// </summary>
		private readonly int sampleRate;

		/// <summary>
		/// Defines the number of channels of the audio input (1=mono, 2=stereo).
		/// </summary>
		private readonly int channels;

		/// <summary>
		/// Defines the number of frames per speex packet.
		/// </summary>
		private readonly int nframes;

		/// <summary>
		/// Defines whether or not to use VBR (Variable Bit Rate).
		/// </summary>
		private readonly bool vbr;

		private bool isPCM;

		private int size;

		/// <summary>
		/// Constructor for PCM Wave file.
		/// </summary>
		/// <param name="sampleRate">the number of samples per second.</param>
		/// <param name="channels">the number of audio channels (1=mono, 2=stereo, ...).</param>
		public PcmWaveWriter(int sampleRate, int channels)
		{
            this.sampleRate = sampleRate;
			this.channels = channels;
			isPCM = true;
		}

		/// <summary>
		/// Constructor for a Speex Wave file.
		/// </summary>
		/// <param name="mode">the mode of the encoder (0=NB, 1=WB, 2=UWB).</param>
		/// <param name="quality"></param>
		/// <param name="sampleRate">the number of samples per second.</param>
		/// <param name="channels">the number of audio channels (1=mono, 2=stereo, ...).</param>
		/// <param name="nframes">the number of frames per speex packet.</param>
		/// <param name="vbr"></param>
		public PcmWaveWriter(
			int mode, int quality,
			int sampleRate, int channels, int nframes,
			bool vbr)
        {
            this.mode = mode;
			this.quality = quality;
			this.sampleRate = sampleRate;
			this.channels = channels;
			this.nframes = nframes;
			this.vbr = vbr;
			isPCM = false;
		}

		/// <summary>
		/// Closes the output file. MUST be called to have a correct stream.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public override void Close()
		{
			// Update the total file length field from RIFF chunk
			raf.BaseStream.Seek(4, System.IO.SeekOrigin.Begin);
			int fileLength = (int)raf.BaseStream.Length - 8;
			raf.Write(fileLength);

			// Update the data chunk length size
			raf.BaseStream.Seek(40, System.IO.SeekOrigin.Begin);
			raf.Write(size);

			// Close the output file
			raf.Close();
		}

		/// <summary>
		/// Open the output file.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public override void Open(Stream stream)
		{
			raf = new BinaryWriter(stream);
			size = 0;
		}

		/// <summary>
		/// Writes the initial data chunks that start the wave file. Prepares file
		/// for data samples to written.
		/// </summary>
		/// <param name="comment">ignored by the WAV header.</param>
		/// <exception cref="IOException"></exception>
		public override void WriteHeader(String comment)
		{
			// Writes the RIFF chunk indicating wave format
            byte[] chkid = System.Text.Encoding.UTF8.GetBytes("RIFF");
			raf.Write(chkid, 0, chkid.Length);
			raf.Write(0); /* total length must be blank */
            chkid = System.Text.Encoding.UTF8.GetBytes("WAVE");
			raf.Write(chkid, 0, chkid.Length);

			/* format subchunk: of size 16 */
            chkid = System.Text.Encoding.UTF8.GetBytes("fmt ");
			raf.Write(chkid, 0, chkid.Length);
			if (isPCM)
			{
				raf.Write(16); // Size of format chunk
				raf.Write(WAVE_FORMAT_PCM); // Format tag: PCM
				raf.Write((short)channels); // Number of channels
				raf.Write(sampleRate); // Sampling frequency
				raf.Write(sampleRate * channels * 2); // Average bytes per second
				raf.Write((short)(channels * 2)); // Blocksize of data
				raf.Write((short)16); // Bits per sample
			}
			else
			{
				int length = comment.Length;
				raf.Write((short)(18 + 2 + 80 + length)); // Size of format chunk
				raf.Write(WAVE_FORMAT_SPEEX); // Format tag: Speex
				raf.Write((short)channels); // Number of channels
				raf.Write(sampleRate); // Sampling frequency
				raf.Write((CalculateEffectiveBitrate(mode, channels, quality) + 7) >> 3); // Average bytes per second
				raf.Write((short)CalculateBlockSize(mode, channels, quality)); // Blocksize of data
				raf.Write((short)quality); // Bits per sample
				raf.Write((short)(2 + 80 + length)); // The count in bytes of the extra size
				// FIXME: Probably wrong!!!
				raf.Write(0xff & 1); // ACM major version number
				raf.Write(0xff & 0); // ACM minor version number
				raf.Write(NSpeex.AudioFileWriter.BuildSpeexHeader(sampleRate, mode,
						channels, vbr, nframes));
				raf.Write(comment);
			}

			/* write the start of data chunk */
            chkid = System.Text.Encoding.UTF8.GetBytes("data");
			raf.Write(chkid, 0, chkid.Length);
			raf.Write(0);
		}

		/// <summary>
		/// Writes a packet of audio.
		/// </summary>
		/// <param name="data">audio data</param>
		/// <param name="offset">the offset from which to start reading the data.</param>
		/// <param name="len">the length of data to read.</param>
		/// <exception cref="IOException"></exception>
		public override void WritePacket(byte[] data, int offset, int len)
		{
			raf.Write(data, offset, len);
			size += len;
		}

		/// <summary>
		/// Calculates effective bitrate (considering padding). See end of file for
		/// exerpt from SpeexACM code for more explanations.
		/// </summary>
		/// <returns>effective bitrate (considering padding).</returns>
		private static int CalculateEffectiveBitrate(int mode, int channels, int quality)
		{
			return ((((WAVE_FRAME_SIZES[mode, channels - 1, quality] * WAVE_BITS_PER_FRAME[mode, channels - 1, quality]) + 7) >> 3) * 50 * 8)
					/ WAVE_BITS_PER_FRAME[mode, channels - 1, quality];
		}

		/// <summary>
		/// Calculates block size (considering padding). See end of file for exerpt
		/// from SpeexACM code for more explanations.
		/// </summary>
		/// <returns>block size (considering padding).</returns>
		private static int CalculateBlockSize(int mode,
				int channels, int quality)
		{
			return (((WAVE_FRAME_SIZES[mode, channels - 1, quality] * WAVE_BITS_PER_FRAME[mode, channels - 1, quality]) + 7) >> 3);
		}
	}

	// The following is taken from the SpeexACM 1.0.1.1 Source code (codec.c file).

	//
	// This array describes how many bits are required by an encoded audio frame.
	// It also specifies the optimal framesperblock parameter to minimize
	// padding loss. It also lists the effective bitrate (considering padding).
	//
	// The array indices are rate, channels, quality (each as a 0 based index)
	//
	/*
	 * struct tagQualityInfo { UINT nBitsPerFrame; UINT nFrameSize; UINT
	 * nFramesPerBlock; UINT nEffectiveBitrate; } QualityInfo[3][2][11] = { 43, 160,
	 * 8, 2150, // 8000 1 0 79, 160, 8, 3950, // 8000 1 1 119, 160, 8, 5950, // 8000
	 * 1 2 160, 160, 1, 8000, // 8000 1 3 160, 160, 1, 8000, // 8000 1 4 220, 160,
	 * 2, 11000, // 8000 1 5 220, 160, 2, 11000, // 8000 1 6 300, 160, 2, 15000, //
	 * 8000 1 7 300, 160, 2, 15000, // 8000 1 8 364, 160, 2, 18200, // 8000 1 9 492,
	 * 160, 2, 24600, // 8000 1 10 60, 160, 2, 3000, // 8000 2 0 96, 160, 1, 4800, //
	 * 8000 2 1 136, 160, 1, 6800, // 8000 2 2 177, 160, 7, 8857, // 8000 2 3 177,
	 * 160, 7, 8857, // 8000 2 4 237, 160, 8, 11850, // 8000 2 5 237, 160, 8, 11850, //
	 * 8000 2 6 317, 160, 8, 15850, // 8000 2 7 317, 160, 8, 15850, // 8000 2 8 381,
	 * 160, 3, 19066, // 8000 2 9 509, 160, 3, 25466, // 8000 2 10 79, 320, 8, 3950, //
	 * 16000 1 0 115, 320, 8, 5750, // 16000 1 1 155, 320, 8, 7750, // 16000 1 2
	 * 196, 320, 2, 9800, // 16000 1 3 256, 320, 1, 12800, // 16000 1 4 336, 320, 1,
	 * 16800, // 16000 1 5 412, 320, 2, 20600, // 16000 1 6 476, 320, 2, 23800, //
	 * 16000 1 7 556, 320, 2, 27800, // 16000 1 8 684, 320, 2, 34200, // 16000 1 9
	 * 844, 320, 2, 42200, // 16000 1 10 96, 320, 1, 4800, // 16000 2 0 132, 320, 2,
	 * 6600, // 16000 2 1 172, 320, 2, 8600, // 16000 2 2 213, 320, 8, 10650, //
	 * 16000 2 3 273, 320, 7, 13657, // 16000 2 4 353, 320, 6, 17666, // 16000 2 5
	 * 429, 320, 3, 21466, // 16000 2 6 493, 320, 3, 24666, // 16000 2 7 573, 320,
	 * 3, 28666, // 16000 2 8 701, 320, 3, 35066, // 16000 2 9 861, 320, 3, 43066, //
	 * 16000 2 10 83, 640, 8, 4150, // 32000 1 0 151, 640, 8, 7550, // 32000 1 1
	 * 191, 640, 8, 9550, // 32000 1 2 232, 640, 1, 11600, // 32000 1 3 292, 640, 2,
	 * 14600, // 32000 1 4 372, 640, 2, 18600, // 32000 1 5 448, 640, 1, 22400, //
	 * 32000 1 6 512, 640, 1, 25600, // 32000 1 7 592, 640, 1, 29600, // 32000 1 8
	 * 720, 640, 1, 36000, // 32000 1 9 880, 640, 1, 44000, // 32000 1 10 100, 640,
	 * 2, 5000, // 32000 2 0 168, 640, 1, 8400, // 32000 2 1 208, 640, 1, 10400, //
	 * 32000 2 2 249, 640, 7, 12457, // 32000 2 3 309, 640, 8, 15450, // 32000 2 4
	 * 389, 640, 3, 19466, // 32000 2 5 465, 640, 6, 23266, // 32000 2 6 529, 640,
	 * 6, 26466, // 32000 2 7 609, 640, 5, 30480, // 32000 2 8 737, 640, 5, 36880, //
	 * 32000 2 9 897, 640, 5, 44880, // 32000 2 10 };
	 */
}
