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
	/// Ogg Speex Writer
	/// </summary>
	public class OggSpeexWriter : AudioFileWriter
	{
		/// <summary>
		/// Number of packets in an Ogg page (must be less than 255)
		/// </summary>
		public const int PACKETS_PER_OGG_PAGE = 250;

		/// <summary>
		/// The OutputStream
		/// </summary>
		private BinaryWriter xout;

		/// <summary>
		/// Defines the encoder mode (0=NB, 1=WB and 2-UWB).
		/// </summary>
		private readonly int mode;

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

		/// <summary>
		/// Ogg Stream Serial Number
		/// </summary>
		private int streamSerialNumber;

		/// <summary>
		/// Data buffer
		/// </summary>
		private byte[] dataBuffer;

		/// <summary>
		/// Pointer within the Data buffer
		/// </summary>
		private int dataBufferPtr;

		/// <summary>
		/// Header buffer
		/// </summary>
		private byte[] headerBuffer;

		/// <summary>
		/// Pointer within the Header buffer
		/// </summary>
		private int headerBufferPtr;

		/// <summary>
		/// Ogg Page count
		/// </summary>
		private int pageCount;

		/// <summary>
		/// Speex packet count within an Ogg Page
		/// </summary>
		private int packetCount;

		/// <summary>
		/// Absolute granule position (the number of audio samples from beginning of
		/// file to end of Ogg Packet).
		/// </summary>
		private long granulepos;

		/// <summary>
		/// Builds an Ogg Speex Writer.
		/// </summary>
		/// <param name="mode">the mode of the encoder (0=NB, 1=WB, 2=UWB).</param>
		/// <param name="sampleRate">the number of samples per second.</param>
		/// <param name="channels">the number of audio channels (1=mono, 2=stereo, ...).</param>
		/// <param name="nframes">the number of frames per speex packet.</param>
		public OggSpeexWriter(
			int mode, int sampleRate,
			int channels, int nframes, bool vbr)
		{
			streamSerialNumber = new Random().Next();
			dataBuffer = new byte[65565];
			dataBufferPtr = 0;
			headerBuffer = new byte[255];
			headerBufferPtr = 0;
			pageCount = 0;
			packetCount = 0;
			granulepos = 0;

			this.mode = mode;
			this.sampleRate = sampleRate;
			this.channels = channels;
			this.nframes = nframes;
			this.vbr = vbr;
		}

		public int SerialNumber
		{
			set
			{
				this.streamSerialNumber = value;
			}
		}

		/// <summary>
		/// Closes the output file.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public override void Close()
		{
			Flush(true);
			xout.Close();
		}

		/// <summary>
		/// Open the output file.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public override void Open(Stream stream)
		{
			xout = new BinaryWriter(stream);
		}

		/// <summary>
		/// Writes an Ogg Page Header to the given byte array.
		/// </summary>
		///
		/// <param name="buf">the buffer to write to.</param>
		/// <param name="offset">the from which to start writing.</param>
		/// <param name="headerType">the header type flag (0=normal, 2=bos: beginning of stream,</param>
		/// <param name="granulepos">the absolute granule position.</param>
		/// <param name="streamSerialNumber"></param>
		/// <param name="pageCount"></param>
		/// <param name="packetCount"></param>
		/// <param name="packetSizes"></param>
		/// <returns>the amount of data written to the buffer.</returns>
		private static int WriteOggPageHeader(BinaryWriter buf,
				int headerType, long granulepos, int streamSerialNumber,
				int pageCount, int packetCount, byte[] packetSizes)
		{
            buf.Write(System.Text.Encoding.UTF8.GetBytes("OggS")); // 0 - 3: capture_pattern
			buf.Write(Byte.MinValue); // 4: stream_structure_version
			buf.Write((byte)headerType); // 5: header_type_flag
			buf.Write(granulepos); // 6 - 13: absolute granule
			// position
			buf.Write(streamSerialNumber); // 14 - 17: stream
			// serial number
			buf.Write(pageCount); // 18 - 21: page sequence no
			buf.Write(0); // 22 - 25: page checksum
			buf.Write((byte)packetCount); // 26: page_segments
			buf.Write(packetSizes, 0, packetCount);
			return packetCount + 27;
		}

		/// <summary>
		/// Builds and returns an Ogg Page Header.
		/// </summary>
		///
		/// <param name="headerType">the header type flag (0=normal, 2=bos: beginning of stream,</param>
		/// <param name="granulepos">the absolute granule position.</param>
		/// <param name="streamSerialNumber"></param>
		/// <param name="pageCount"></param>
		/// <param name="packetCount"></param>
		/// <param name="packetSizes"></param>
		/// <returns>an Ogg Page Header.</returns>
		private static byte[] BuildOggPageHeader(int headerType, long granulepos,
				int streamSerialNumber, int pageCount, int packetCount,
				byte[] packetSizes)
		{
			byte[] data = new byte[packetCount + 27];
			WriteOggPageHeader(new BinaryWriter(new MemoryStream(data)), headerType, granulepos, streamSerialNumber,
					pageCount, packetCount, packetSizes);
			return data;
		}

		/// <summary>
		/// Writes the header pages that start the Ogg Speex file. Prepares file for
		/// data to be written.
		/// </summary>
		///
		/// <param name="comment">description to be included in the header.</param>
		/// @exception IOException
		public override void WriteHeader(String comment)
		{
			byte[] header;
			byte[] data;
			NSpeex.OggCrc crc = new NSpeex.OggCrc();
			/* writes the OGG header page */
			header = BuildOggPageHeader(2, 0,
					streamSerialNumber, pageCount++, 1, new byte[] { 80 });
			data = NSpeex.AudioFileWriter.BuildSpeexHeader(sampleRate, mode,
					channels, vbr, nframes);
			crc.Initialize();
			crc.TransformBlock(header, 0, header.Length, header, 0);
			crc.TransformFinalBlock(data, 0, data.Length);
			xout.Write(header, 0, 22);
			xout.Write(crc.Hash, 0, crc.HashSize / 8);
			xout.Write(header, 26, header.Length - 26);
			xout.Write(data, 0, data.Length);
			/* writes the OGG comment page */
			header = BuildOggPageHeader(0, 0,
					streamSerialNumber, pageCount++, 1,
					new byte[] { (byte)(comment.Length + 8) });
			data = NSpeex.AudioFileWriter.BuildSpeexComment(comment);
			crc.Initialize();
			crc.TransformBlock(header, 0, header.Length, header, 0);
			crc.TransformFinalBlock(data, 0, data.Length);
			xout.Write(header, 0, 22);
			xout.Write(crc.Hash, 0, crc.HashSize / 8);
			xout.Write(header, 26, header.Length - 26);
			xout.Write(data, 0, data.Length);
		}

		/// <summary>
		/// Writes a packet of audio.
		/// </summary>
		///
		/// <param name="data"> -</param>
		/// <param name="offset"> -</param>
		/// <param name="len"> -</param>
		/// @exception IOException
		public override void WritePacket(byte[] data, int offset, int len)
		{
			if (len <= 0)
			{ // nothing to write
				return;
			}
			if (packetCount > PACKETS_PER_OGG_PAGE)
			{
				Flush(false);
			}
			System.Array.Copy(data, offset, dataBuffer, dataBufferPtr, len);
			dataBufferPtr += len;
			headerBuffer[headerBufferPtr++] = (byte)len;
			packetCount++;
			granulepos += nframes * ((mode == 2) ? 640 : ((mode == 1) ? 320 : 160));
		}

		/// <summary>
		/// Flush the Ogg page out of the buffers into the file.
		/// </summary>
		///
		/// <param name="eos"> -</param>
		/// @exception IOException
		private void Flush(bool eos)
		{
			byte[] header;
			NSpeex.OggCrc crc = new NSpeex.OggCrc();
			/* writes the OGG header page */
			header = BuildOggPageHeader(((eos) ? 4 : 0),
					granulepos, streamSerialNumber, pageCount++, packetCount,
					headerBuffer);
			crc.Initialize();
			crc.TransformBlock(header, 0, header.Length, header, 0);
			crc.TransformFinalBlock(dataBuffer, 0, dataBufferPtr);
			xout.Write(header, 0, 22);
			xout.Write(crc.Hash, 0, crc.HashSize / 8);
			xout.Write(header, 26, header.Length - 26);
			xout.Write(dataBuffer, 0, dataBufferPtr);
			dataBufferPtr = 0;
			headerBufferPtr = 0;
			packetCount = 0;
		}
	}
}
