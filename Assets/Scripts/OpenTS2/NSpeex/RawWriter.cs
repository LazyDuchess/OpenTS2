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
	/// Raw Audio File Writer.
	/// </summary>
	public class RawWriter : AudioFileWriter
	{
		private Stream xout;

		/// <summary>
		/// Closes the output file.
		/// </summary>
		public override void Close()
		{
			xout.Close();
		}

		/// <summary>
		/// Open the output file.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public override void Open(Stream stream)
		{
			xout = stream;
		}

		/// <summary>
		/// Writes the header pages that start the Ogg Speex file. Prepares file for
		/// data to be written.
		/// </summary>
		/// <param name="comment">description to be included in the header.</param>
		/// <exception cref="IOException"></exception>
		public override void WriteHeader(String comment)
		{
			// a raw audio file has no header
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
			xout.Write(data, offset, len);
		}
	}
}
