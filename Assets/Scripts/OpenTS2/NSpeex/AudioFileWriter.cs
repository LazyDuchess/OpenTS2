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
	/// Abstract Class that defines an Audio File Writer.
	/// </summary>
	public abstract class AudioFileWriter
	{
		/// <summary>
		/// Closes the output file.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public abstract void Close();

		/// <summary>
		/// Open the output file.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public abstract void Open(Stream stream);

		/// <summary>
		/// Open the output file.
		/// </summary>
		/// <param name="filename"> -</param>
		/// <exception cref="IOException"></exception>
		public void Open(string filename)
		{
			Open(new FileStream(filename, FileMode.Create));
		}

		/// <summary>
		/// Writes the header pages that start the Ogg Speex file. Prepares file for
		/// data to be written.
		/// </summary>
		/// <param name="comment">description to be included in the header.</param>
		/// <exception cref="IOException"></exception>
		public abstract void WriteHeader(String comment);

		/// <summary>
		/// Writes a packet of audio.
		/// </summary>
		/// <param name="data">audio data</param>
		/// <param name="offset">the offset from which to start reading the data.</param>
		/// <param name="len">the length of data to read.</param>
		/// <exception cref="IOException"></exception>
		public abstract void WritePacket(byte[] data, int offset, int len);

		/// <summary>
		/// Writes a Speex Header to the given byte array.
		/// </summary>
		/// <param name="buf">the buffer to write to.</param>
		/// <param name="offset">the from which to start writing.</param>
		/// <returns>the amount of data written to the buffer.</returns>
		protected static int WriteSpeexHeader(
			BinaryWriter buf,
			int sampleRate,
			int mode,
			int channels,
			bool vbr,
			int nframes)
		{
			buf.Write(System.Text.Encoding.UTF8.GetBytes("Speex   ")); // 0 - 7: speex_string
            buf.Write(System.Text.Encoding.UTF8.GetBytes("speex-1.0")); // 8 - 27: speex_version
			for (int i = 0; i < 11; i++)
				buf.Write(Byte.MinValue); // (fill in up to 20 bytes)
			buf.Write(1); // 28 - 31: speex_version_id
			buf.Write(80); // 32 - 35: header_size
			buf.Write(sampleRate); // 36 - 39: rate
			buf.Write(mode); // 40 - 43: mode (0=NB, 1=WB, 2=UWB)
			buf.Write(4); // 44 - 47: mode_bitstream_version
			buf.Write(channels); // 48 - 51: nb_channels
			buf.Write(-1); // 52 - 55: bitrate
			buf.Write(160 << mode); // 56 - 59: frame_size
			// (NB=160, WB=320, UWB=640)
			buf.Write((vbr) ? 1 : 0); // 60 - 63: vbr
			buf.Write(nframes); // 64 - 67: frames_per_packet
			buf.Write(0); // 68 - 71: extra_headers
			buf.Write(0); // 72 - 75: reserved1
			buf.Write(0); // 76 - 79: reserved2
			return 80;
		}

		/// <summary>
		/// Builds a Speex Header.
		/// </summary>
		/// <returns>a Speex Header.</returns>
		protected static byte[] BuildSpeexHeader(
			int sampleRate, int mode,
			int channels, bool vbr, int nframes)
		{
			byte[] data = new byte[80];
			WriteSpeexHeader(new BinaryWriter(new MemoryStream(data)), sampleRate, mode, channels, vbr, nframes);
			return data;
		}

		/// <summary>
		/// Writes a Speex Comment to the given byte array.
		/// </summary>
		/// <param name="buf">the buffer to write to.</param>
		/// <param name="offset">the from which to start writing.</param>
		/// <param name="comment">the comment.</param>
		/// <returns>the amount of data written to the buffer.</returns>
		protected static int WriteSpeexComment(BinaryWriter buf, String comment)
		{
			int length = comment.Length;
			buf.Write(length); // vendor comment size
            buf.Write(System.Text.Encoding.UTF8.GetBytes(comment), 0, length); // vendor comment
			buf.Write(0); // user comment list length
			return length + 8;
		}

		/// <summary>
		/// Builds and returns a Speex Comment.
		/// </summary>
		/// <param name="comment">the comment.</param>
		/// <returns>a Speex Comment.</returns>
		protected static byte[] BuildSpeexComment(String comment)
		{
			byte[] data = new byte[comment.Length + 8];
			WriteSpeexComment(new BinaryWriter(new MemoryStream(data)), comment);
			return data;
		}
	}
}
