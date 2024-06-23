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

namespace NSpeex
{
	/// <summary>
	/// Speex in-band and User in-band controls.
	/// </summary>
    internal class Inband
	{
		private Stereo stereo;

		public Inband(Stereo stereo)
		{
			this.stereo = stereo;
		}

		/// <summary>
		/// Speex in-band request (submode=14).
		/// </summary>
		public void SpeexInbandRequest(Bits bits)
		{
			int code = bits.Unpack(4);
			switch (code)
			{
				case 0: // asks the decoder to set perceptual enhancment off (0) or on
					// (1)
					bits.Advance(1);
					break;
				case 1: // asks (if 1) the encoder to be less "aggressive" due to high
					// packet loss
					bits.Advance(1);
					break;
				case 2: // asks the encoder to switch to mode N
					bits.Advance(4);
					break;
				case 3: // asks the encoder to switch to mode N for low-band
					bits.Advance(4);
					break;
				case 4: // asks the encoder to switch to mode N for high-band
					bits.Advance(4);
					break;
				case 5: // asks the encoder to switch to quality N for VBR
					bits.Advance(4);
					break;
				case 6: // request acknowledgement (0=no, 1=all, 2=only for inband data)
					bits.Advance(4);
					break;
				case 7: // asks the encoder to set CBR(0), VAD(1), DTX(3), VBR(5),
					// VBR+DTX(7)
					bits.Advance(4);
					break;
				case 8: // transmit (8-bit) character to the other end
					bits.Advance(8);
					break;
				case 9: // intensity stereo information
					// setup the stereo decoder; to skip: tmp = bits.unpack(8); break;
					stereo.Init(bits); // read 8 bits
					break;
				case 10: // announce maximum bit-rate acceptable (N in byets/second)
					bits.Advance(16);
					break;
				case 11: // reserved
					bits.Advance(16);
					break;
				case 12: // Acknowledge receiving packet N
					bits.Advance(32);
					break;
				case 13: // reserved
					bits.Advance(32);
					break;
				case 14: // reserved
					bits.Advance(64);
					break;
				case 15: // reserved
					bits.Advance(64);
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// User in-band request (submode=13).
		/// </summary>
		public void UserInbandRequest(Bits bits)
		{
			int req_size = bits.Unpack(4);
			bits.Advance(5 + 8 * req_size);
		}
	}
}
