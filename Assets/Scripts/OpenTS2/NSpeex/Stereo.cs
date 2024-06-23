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

using System.Collections.Generic;
using System.Collections;
using System;
#if FIXED_POINT
using SpeexWord16 = System.Int16;
using SpeexWord32 = System.Int32;
#else
using SpeexWord16 = System.Single;
using SpeexWord32 = System.Single;
#endif

namespace NSpeex
{
    internal class Stereo
	{
		public Stereo()
		{
#if FIXED_POINT
			this.smooth_right = 1 << 14;
			this.smooth_left = 1 << 14;
			this.e_ratio = 1 << 14;
			this.balance = 1 << 16;
#else
			this.smooth_right = 1f;
			this.smooth_left = 1f;
			this.e_ratio = 0.5f;
			this.balance = 1f;
#endif
		}

		/// <summary>
		/// Inband code number for Stereo
		/// </summary>
		///
		private const int SPEEX_INBAND_STEREO = 9;
#if FIXED_POINT
		public static readonly SpeexWord16[] e_ratio_quant = {8192, 10332, 13009, 16384};
		public static readonly SpeexWord16[] e_ratio_quant_bounds = {9257, 11665, 14696};
		public static readonly SpeexWord16[] balance_bounds = {
			18, 23, 30, 38, 49, 63,  81, 104,
			134, 172, 221,  284, 364, 468, 600, 771,
			990, 1271, 1632, 2096, 2691, 3455, 4436, 5696,
			7314, 9392, 12059, 15484, 19882, 25529, 32766};
#else
		private static readonly float[] e_ratio_quant = { .25f, .315f, .397f, .5f };
#endif

		/// <summary>
		/// Left/right balance info
		/// </summary>
		private SpeexWord32 balance;

		/// <summary>
		/// Ratio of energies: E(left+right)/[E(left)+E(right)]
		/// </summary>
		private SpeexWord32 e_ratio;

		/// <summary>
		/// Smoothed left channel gain
		/// </summary>
		private SpeexWord32 smooth_left;

		/// <summary>
		/// Smoothed right channel gain
		/// </summary>
		private SpeexWord32 smooth_right;

		/// <summary>
		/// Transforms a stereo frame into a mono frame and stores intensity stereo
		/// info in 'bits'.
		/// </summary>
		public static void Encode(Bits bits, SpeexWord16[] data, int frameSize)
		{
#if FIXED_POINT
			int i, tmp;
			SpeexWord32 e_left = 0, e_right = 0, e_tot = 0;
			SpeexWord32 balance, e_ratio;
			SpeexWord32 largest, smallest;
			int shift;
			int balance_id;

			/* In band marker */
			bits.Pack(14, 5);
			/* Stereo marker */
			bits.Pack(SPEEX_INBAND_STEREO, 4);

			for (i = 0; i < frameSize; i++)
			{
				e_left += (data[2 * i] * data[2 * i]) >> 8;
				e_right += (data[2 * i + 1] * data[2 * i + 1]) >> 8;
				data[i] = (SpeexWord16)((data[2 * i] >> 1) + (data[2 * i + 1] >> 1));
				e_tot += (data[i] * data[i]) >> 8;
			}

			if (e_left > e_right)
			{
				bits.Pack(0, 1);
				largest = e_left;
				smallest = e_right;
			}
			else
			{
				bits.Pack(1, 1);
				largest = e_right;
				smallest = e_left;
			}

			/* Balance quantization */
			// ???
			shift = (int)Math.Log(smallest, 2) - 15;
			largest >>= shift;
			smallest >>= shift;
			balance = Math.Min(largest / (smallest + 1), 32767);
			balance_id = VQ.Index((short)balance, balance_bounds, balance_bounds.Length);
			bits.Pack((int)balance_id, 5);

			/* "coherence" quantisation */
			shift = (int)Math.Log(e_tot, 2);
			e_tot >>= shift - 25;
			e_left >>= shift - 10;
			e_right >>= shift - 10;
			e_ratio = e_tot / (e_left + e_right + 1);

			tmp = NSpeex.VQ.Index((short)e_ratio, e_ratio_quant_bounds, 4);
			bits.Pack(tmp, 2);
#else
			int i, tmp;
			float e_left = 0, e_right = 0, e_tot = 0;
			float balance_0, e_ratio_1;
			for (i = 0; i < frameSize; i++)
			{
				e_left += data[2 * i] * data[2 * i];
				e_right += data[2 * i + 1] * data[2 * i + 1];
				data[i] = .5f * (data[2 * i] + data[2 * i + 1]);
				e_tot += data[i] * data[i];
			}
			balance_0 = (e_left + 1) / (e_right + 1);
			e_ratio_1 = e_tot / (1 + e_left + e_right);
			/* Quantization */
			bits.Pack(14, 5);
			bits.Pack(SPEEX_INBAND_STEREO, 4);
			balance_0 = (float)(4 * Math.Log(balance_0));

			/* Pack balance */
			if (balance_0 > 0)
				bits.Pack(0, 1);
			else
				bits.Pack(1, 1);
			balance_0 = (float)Math.Floor(.5f + Math.Abs(balance_0));
			if (balance_0 > 30)
				balance_0 = 31;
			bits.Pack((int)balance_0, 5);

			/* Quantize energy ratio */
			tmp = NSpeex.VQ.Index(e_ratio_1, e_ratio_quant, 4);
			bits.Pack(tmp, 2);
#endif
		}

		/// <summary>
		/// Transforms a mono frame into a stereo frame using intensity stereo info.
		/// </summary>
		///
		/// <param name="data"> -</param>
		/// <param name="frameSize"> -</param>
		public void Decode(float[] data, int frameSize)
		{
#if !FIXED_POINT_TODO
			int i;
			float e_tot = 0, e_left, e_right, e_sum;

			for (i = frameSize - 1; i >= 0; i--)
			{
				e_tot += data[i] * data[i];
			}
			e_sum = e_tot / e_ratio;
			e_left = e_sum * balance / (1 + balance);
			e_right = e_sum - e_left;
			e_left = (float)Math.Sqrt(e_left / (e_tot + .01f));
			e_right = (float)Math.Sqrt(e_right / (e_tot + .01f));

			for (i = frameSize - 1; i >= 0; i--)
			{
				float ftmp = data[i];
				smooth_left = .98f * smooth_left + .02f * e_left;
				smooth_right = .98f * smooth_right + .02f * e_right;
				data[2 * i] = smooth_left * ftmp;
				data[2 * i + 1] = smooth_right * ftmp;
			}
#endif
		}

		/// <summary>
		/// Callback handler for intensity stereo info
		/// </summary>
		public void Init(Bits bits)
		{
			SpeexWord16 sign = 1;
			int tmp;
			if (bits.Unpack(1) != 0)
				sign = -1;
			tmp = bits.Unpack(5);
#if FIXED_POINT
			balance = (int)Math.Exp(sign * (tmp << 9));
#else
			balance = (float)Math.Exp(sign * .25d * tmp);
#endif
			tmp = bits.Unpack(2);
			e_ratio = e_ratio_quant[tmp];
		}
	}
}
