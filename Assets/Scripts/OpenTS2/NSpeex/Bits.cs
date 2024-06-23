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

using System;

namespace NSpeex
{
	/// <summary>
	/// Speex bit packing and unpacking class.
	/// </summary>
	internal class Bits
	{
		/// <summary>
		/// Default buffer size
		/// </summary>
		///
		public const int DefaultBufferSize = 1024;

		/// <summary>
		/// "raw" data
		/// </summary>
		private byte[] bytes;

		/// <summary>
		/// Position of the byte "cursor"
		/// </summary>
		private int bytePtr;

		/// <summary>
		/// Position of the bit "cursor" within the current byte
		/// </summary>
		private int bitPtr;

	    private int nbBits;

		/// <summary>
		/// Initialise the bit packing variables.
		/// </summary>
		public Bits()
		{
			bytes = new byte[DefaultBufferSize];
			bytePtr = 0;
			bitPtr = 0;
		}

		/// <summary>
		/// Advance n bits.
		/// </summary>
		public void Advance(int n)
		{
			bytePtr += n >> 3;
			bitPtr += n & 7;
			if (bitPtr > 7)
			{
				bitPtr -= 8;
				bytePtr++;
			}
		}

		/// <summary>
		/// Take a peek at the next bit.
		/// </summary>
		public int Peek()
		{
			return ((bytes[bytePtr] & 0xFF) >> (7 - bitPtr)) & 1;
		}

		/// <summary>
		/// Read the given array into the buffer.
		/// </summary>
		public void ReadFrom(byte[] newbytes, int offset, int len)
		{
            // resize if needed
            if (bytes.Length < len)
                bytes = new byte[len];

			for (int i = 0; i < len; i++)
				bytes[i] = newbytes[offset + i];
			bytePtr = 0;
			bitPtr = 0;
		    nbBits = len*8;
		}

        public int BitsRemaining()
        {
            return nbBits - (bytePtr*8 + bitPtr);
        }

		/// <summary>
		/// Read the next N bits from the buffer.
		/// </summary>
		/// <returns>the next N bits from the buffer.</returns>
		public int Unpack(int nbBits)
		{
			int d = 0;
			while (nbBits != 0)
			{
				d <<= 1;
				d |= ((bytes[bytePtr] & 0xFF) >> (7 - bitPtr)) & 1;
				bitPtr++;
				if (bitPtr == 8)
				{
					bitPtr = 0;
					bytePtr++;
				}
				nbBits--;
			}
			return d;
		}

		/// <summary>
		/// Write N bits of the given data to the buffer.
		/// </summary>
		public void Pack(int data, int nbBits)
		{
			int d = data;

			while (bytePtr + ((nbBits + bitPtr) >> 3) >= bytes.Length)
			{
				// Expand the buffer as needed.
				int size = bytes.Length * 2;
				byte[] tmp = new byte[size];
				Array.Copy(bytes, 0, tmp, 0, bytes.Length);
				bytes = tmp;
			}
			while (nbBits > 0)
			{
				int bit;
				bit = (d >> (nbBits - 1)) & 1;
				bytes[bytePtr] |= (byte)(bit << (7 - bitPtr));
				bitPtr++;
				if (bitPtr == 8)
				{
					bitPtr = 0;
					bytePtr++;
				}
				nbBits--;
			}
		}

        public void InsertTerminator()
        {
            if (bitPtr > 0)
                Pack(0, 1);
            while (bitPtr != 0)
                Pack(1, 1);
        }

        public int Write(byte[] buffer, int offset, int maxBytes)
        {
            // insert terminator, but save the data so we can put it back after
            var localBitPtr = bitPtr;
            var localBytePtr = bytePtr;
            var localBytes = bytes;
            InsertTerminator();
            bitPtr = localBitPtr;
            bytePtr = localBytePtr;
            bytes = localBytes;

            if (maxBytes > BufferSize)
                maxBytes = BufferSize;

            Array.Copy(bytes, 0, buffer, offset, maxBytes);
            return maxBytes;
        }

        public void Reset()
        {
            Array.Clear(bytes, 0, bytes.Length);
            bytePtr = 0;
            bitPtr = 0;
        }

		public int BufferSize
		{
			get { return bytePtr + ((bitPtr > 0) ? 1 : 0); }
		}
	}
}
