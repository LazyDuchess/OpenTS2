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
	/// Long Term Prediction Quantisation and Unquantisation (Forced Pitch)
	/// </summary>
    internal class LtpForcedPitch : Ltp
	{
		/// <summary>
		/// Long Term Prediction Quantification (Forced Pitch).
		/// </summary>
		/// <returns>pitch</returns>
		public sealed override int Quant(
			float[] target, float[] sw, int sws, float[] ak,
			float[] awk1, float[] awk2, float[] exc, int es, int start,
			int end, float pitch_coef, int p, int nsf, Bits bits, float[] exc2,
			int e2s, float[] r, int complexity)
		{
			int i;
			if (pitch_coef > .99f)
				pitch_coef = .99f;
			for (i = 0; i < nsf; i++)
			{
				exc[es + i] = exc[es + i - start] * pitch_coef;
			}
			return start;
		}

		/// <summary>
		/// Long Term Prediction Unquantification (Forced Pitch).
		/// </summary>
		/// <returns>pitch</returns>
		public sealed override int Unquant(float[] exc, int es, int start, float pitch_coef,
				int nsf, float[] gain_val, Bits bits, int count_lost,
				int subframe_offset, float last_pitch_gain)
		{
			int i;
			if (pitch_coef > .99f)
			{
				pitch_coef = .99f;
			}
			for (i = 0; i < nsf; i++)
			{
				exc[es + i] = exc[es + i - start] * pitch_coef;
			}
			gain_val[0] = gain_val[2] = 0;
			gain_val[1] = pitch_coef;
			return start;
		}
	}
}
