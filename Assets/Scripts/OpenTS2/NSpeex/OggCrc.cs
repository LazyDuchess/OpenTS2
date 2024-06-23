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

using System.Security.Cryptography;
using System;

namespace NSpeex
{
	/// <summary>
	/// Calculates the CRC checksum for Ogg packets.
	/// 
	/// Ogg uses the same generator polynomial as ethernet, although with an
	/// unreflected alg and an init/final of 0, not 0xffffffff.
	/// </summary>
	public class OggCrc : HashAlgorithm
	{
		private const uint Polynomial = 0x04c11db7;
		private const uint Seed = 0;

		/// <summary>
		/// CRC checksum lookup table.
		/// </summary>
		private static uint[] lookupTable;

		/// <summary>
		/// Current value of the CRC checksum.
		/// </summary>
		private uint hash;

		static OggCrc()
		{
			lookupTable = new uint[256];
			for (uint i = 0; i < lookupTable.Length; i++)
			{
				uint entry = i << 24;//i;
				for (int j = 0; j < 8; j++)
				{
					//if ((entry & 1) == 1)
					if ((entry & 0x80000000) != 0)
						entry = (entry << 1) ^ Polynomial;
					else
						entry <<= 1;
				}
				lookupTable[i] = entry;
			}
		}

		public override void Initialize()
		{
			this.hash = Seed;
		}

		/// <summary>
		/// Calculates the checksum on the given data, from the give offset and for
		/// the given length, using the given initial value. This allows on to
		/// calculate the checksum iteratively, by reinjecting the last returned
		/// value as the initial value when the function is called for the next data
		/// chunk. The initial value should be 0 for the first iteration.
		/// </summary>
		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			for (int i = 0; i < cbSize; i++)
			{
				unchecked
				{
					this.hash = (this.hash << 8) ^ lookupTable[(array[i + ibStart] ^ (this.hash >> 24)) & 0xff];
				}
			}
		}

		protected override byte[] HashFinal()
		{
			return new byte[] {
				(byte)(this.hash & 0xff),
				(byte)((this.hash >> 8) & 0xff),
				(byte)((this.hash >> 16) & 0xff),
				(byte)((this.hash >> 24) & 0xff)
			};
		}

		public override int HashSize
		{
			get { return 32; }
		}
	}
}
